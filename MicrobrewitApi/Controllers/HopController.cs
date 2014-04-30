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
using StackExchange.Redis;

namespace Microbrewit.Api.Controllers
{

    [RoutePrefix("hops")]
    public class HopController : ApiController
    {

        private MicrobrewitContext db = new MicrobrewitContext();
        private IHopRepository _hopRepository;
        private static readonly string redisStore = ConfigurationManager.AppSettings["redis"];
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public HopController(IHopRepository hopRepository)
        {
            this._hopRepository = hopRepository;
        }

        // GET api/Hops
        [Route("")]
        public HopCompleteDto GetHops()
        {
            IList<HopDto> hops = new List<HopDto>();
            using (var redis = ConnectionMultiplexer.Connect(redisStore))
            {
                var redisClient = redis.GetDatabase();
                var hopsJson = redisClient.HashGetAll("hops");
                foreach (var hopJson in hopsJson.ToList())
                {
                    hops.Add(JsonConvert.DeserializeObject<HopDto>(hopJson.Value));
                }
            }
            if (hops.Count <= 0)
            {
                hops = Mapper.Map<IList<Hop>, IList<HopDto>>(_hopRepository.GetAll("Flavours.Flavour", "Origin", "Substituts"));
            }
            var result = new HopCompleteDto() { Hops = hops };
            return result;
        }

        // GET api/Hops/5
        [Route("{id:int}")]
        [ResponseType(typeof(HopCompleteDto))]
        [HttpGet]
        public IHttpActionResult GetHop(int id)
        {

            HopDto hop = null;
            hop = GetHopRedis(id);
            if (hop == null)
            {
                hop = Mapper.Map<Hop, HopDto>(_hopRepository.GetSingle(h => h.Id == id, "Flavours.Flavour", "Origin", "Substituts"));
            }

            if (hop == null)
            {
                return NotFound();
            }

            {

            }
            var result = new HopCompleteDto() { Hops = new List<HopDto>() };
            result.Hops.Add(hop);

            return Ok(result);
        }

        private static HopDto GetHopRedis(int id)
        {
            try
            {

                using (var redis = ConnectionMultiplexer.Connect(redisStore))
                {
                    var redisClient = redis.GetDatabase();
                    var hopJson = redisClient.HashGet("hops", id);
                    if (!hopJson.IsNull)
                    {
                        return JsonConvert.DeserializeObject<HopDto>(hopJson);
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception)
            {

                return null;
            }
        }

        // PUT api/Hops/5
        [Route("{id:int}")]
        public IHttpActionResult PutHop(int id, HopDto hopDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != hopDto.Id)
            {
                return BadRequest();
            }
            var hop = Mapper.Map<HopDto, Hop>(hopDto);

            _hopRepository.Update(hop);


            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST api/Hops

        [Route("")]
        [ResponseType(typeof(IList<HopDto>))]
        public IHttpActionResult PostHop(IList<HopDto> HopDtos)
        {
            Log.Debug("Hops Post");
            if (!ModelState.IsValid)
            {
                Log.Debug("Invalid ModelState");

                return BadRequest(ModelState);
            }
            var hops = Mapper.Map<IList<HopDto>, Hop[]>(HopDtos);

            _hopRepository.Add(hops);

            var results = Mapper.Map<IList<Hop>, IList<HopDto>>(_hopRepository.GetAll("Flavours.Flavour", "Origin", "Substituts"));
            using (var redis = ConnectionMultiplexer.Connect(redisStore))
            {

                var redisClient = redis.GetDatabase();

                foreach (var hop in results)
                {
                    redisClient.HashSet("hops", hop.Id, JsonConvert.SerializeObject(hop), flags: CommandFlags.FireAndForget);
                }

            }
            return CreatedAtRoute("DefaultApi", new { controller = "hops", }, results);
        }

        // DELETE api/Hopd/5
        [Route("{id:int}")]
        [ResponseType(typeof(Hop))]
        public IHttpActionResult DeleteHop(int id)
        {
            Hop hop = _hopRepository.GetSingle(h => h.Id == id);
            if (hop == null)
            {
                return NotFound();
            }

            _hopRepository.Remove(hop);
            return Ok(hop);
        }

        [Route("forms")]
        public IList<HopForm> GetHopForm()
        {
            var hopforms = db.HopForms.ToList();
            try
            {
                using (var redis = ConnectionMultiplexer.Connect(redisStore))
                {
                    var redisClient = redis.GetDatabase();
                    foreach (var item in hopforms)
                    {
                        redisClient.HashSet("hopforms", item.Id, JsonConvert.SerializeObject(item), flags: CommandFlags.FireAndForget);
                    }
                }

            }
            catch (RedisConnectionException)
            {
                Log.ErrorFormat("RedisConnectionException was thrown");
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