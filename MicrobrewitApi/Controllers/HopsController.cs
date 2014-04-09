﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microbrewit.Model;
using System.Linq.Expressions;
using log4net;
using Newtonsoft.Json;
using Microbrewit.Model.DTOs;
using AutoMapper;
using Microbrewit.Repository;
using System.Configuration;
using ServiceStack.Redis;

namespace Microbrewit.Api.Controllers
{

    [RoutePrefix("hops")]
    public class HopsController : ApiController
    {

        private MicrobrewitContext db = new MicrobrewitContext();
        private IHopRepository hopRepository = new HopRepository();
        private static readonly string redisStore = ConfigurationManager.AppSettings["redis"];
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        

        // GET api/Hops
        [Route("")]
        public HopCompleteDto GetHops()
        {
            var hops = Mapper.Map<IList<Hop>, IList<HopDto>>(hopRepository.GetAll("Flavours.Flavour", "Origin", "Substituts")); 
            var result = new HopCompleteDto(){Hops = hops};
            return result;
        }

        // GET api/Hops/5
        [Route("{id:int}")]
        [ResponseType(typeof(HopCompleteDto))]
        [HttpGet]
        public IHttpActionResult GetHop(int id)
        {

            var hop = Mapper.Map<Hop, HopDto>(hopRepository.GetSingle(h => h.Id == id, "Flavours.Flavour", "Origin", "Substituts"));

            if (hop == null)
            {
                return NotFound();
            }
            var result = new HopCompleteDto() { Hops = new List<HopDto>() };
            result.Hops.Add(hop);

            return Ok(result);
        }

        [Route("{id:int}/details")]
        [ResponseType(typeof(Hop))]
        public async Task<IHttpActionResult> GetHopDetail(int id)
        {
            var hop = await db.Hops.Include(h => h.Origin).Where(h => h.Id == id).FirstOrDefaultAsync();
            if (hop == null)
            {
                return NotFound();
            }
            return Ok(hop);
        }

        [Route("{origin}")]
        public IQueryable<Hop> GetHopByOrigin(string origin)
        {
            Log.Debug("Origin: " + origin);
            return db.Hops.Include(h => h.Origin)
                .Where(h => h.Origin.Name.Equals(origin, StringComparison.OrdinalIgnoreCase));
        }

        // PUT api/Hops/5
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PutHop(int id, Hop hop)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != hop.Id)
            {
                return BadRequest();
            }

            db.Entry(hop).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HopExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Hops
    
        [Route("")]
        [ResponseType(typeof(IList<HopPostDto>))]
        public IHttpActionResult PostHop(IList<HopPostDto> hopPosts)
        {
            Log.Debug("Hops Post");
            if (!ModelState.IsValid)
            {
                Log.Debug("Invalid ModelState");

                return BadRequest(ModelState);
            }
           var hops = Mapper.Map<IList<HopPostDto>,Hop[]>(hopPosts);

            hopRepository.Add(hops);
            
            var results = Mapper.Map<IList<Hop>, IList<HopDto>>(hopRepository.GetAll("Flavours.Flavour", "Origin", "Substituts"));
            using (var redisClient = new RedisClient(redisStore))
            {
                foreach (var hop in results)
	            {
                    redisClient.SetEntryInHash("hops", hop.Id.ToString(),JsonConvert.SerializeObject(hop));
                }
            }
            return CreatedAtRoute("DefaultApi",new {controller = "hops",},results);
        }

        // DELETE api/Hopd/5
        [Route("{id:int}")]
        [ResponseType(typeof(Hop))]
        public async Task<IHttpActionResult> DeleteHop(int id)
        {
            Hop hop = await db.Hops.FindAsync(id);
            if (hop == null)
            {
                return NotFound();
            }

            db.Hops.Remove(hop);
            await db.SaveChangesAsync();

            return Ok(hop);
        }

        [Route("hopforms")]
        public IList<HopForm> GetHopForm()
        {
            var hopforms = db.HopForms.ToList();
            using (var redisClient = new RedisClient())
            {
                foreach (var item in hopforms)
                {
                    redisClient.SetEntryInHash("hopforms", item.Id.ToString(), JsonConvert.SerializeObject(item));
                }
            }
            return hopforms;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool HopExists(int id)
        {
            return db.Hops.Count(e => e.Id == id) > 0;
        }
    }
}