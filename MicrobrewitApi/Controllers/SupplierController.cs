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
using Microbrewit.Model.DTOs;
using Microbrewit.Repository;
using AutoMapper;

namespace Microbrewit.Api.Controllers
{
    [RoutePrefix("suppliers")]
    public class SupplierController : ApiController
    {
        private readonly ISupplierRepository _supplierRepository;
        private Elasticsearch.ElasticSearch _elasticsearch;

        public SupplierController(ISupplierRepository supplierRepository)
        {
            this._supplierRepository = supplierRepository;
            this._elasticsearch = new Elasticsearch.ElasticSearch();
        }

        /// <summary>
        /// Gets all suppliers
        /// </summary>
        /// <response code="200">OK</response>
        /// <returns></returns>
        [Route("")]
        public async Task<SupplierCompleteDto> GetSuppliers()
        {
            var suppliersDto = await _elasticsearch.GetSuppliers();
            if (suppliersDto.Count() <= 0)
            {
                var suppliers = await _supplierRepository.GetAllAsync("Origin");
                suppliersDto = Mapper.Map<IList<Supplier>, IList<SupplierDto>>(suppliers);
            }
            var result = new SupplierCompleteDto();
            result.Suppliers = suppliersDto.OrderBy(s => s.Name).ToList();
            return result;
        }

        /// <summary>
        /// Get a supplier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="404">Not Found</response>
        /// <param name="id">Supplier id</param>
        /// <returns></returns>
        [Route("{id:int}")]
        [ResponseType(typeof(Supplier))]
        public async Task<IHttpActionResult> GetSupplier(int id)
        {
            var supplierDto = await _elasticsearch.GetSupplier(id);
            if (supplierDto == null)
            {
                var supplier = await _supplierRepository.GetSingleAsync(s => s.Id == id,"Origin");
                supplierDto = Mapper.Map<Supplier, SupplierDto>(supplier);
            }
            if (supplierDto == null)
            {
                return NotFound();
            }
            var result = new SupplierCompleteDto() { Suppliers = new List<SupplierDto>() };
            result.Suppliers.Add(supplierDto);
            return Ok(result);
        }

        /// <summary>
        /// Updates a supplier.
        /// </summary>
        /// <response code="204">No Content</response>
        /// <response code="400">Bad Request</response>
        /// <param name="id">Supplier id</param>
        /// <param name="supplierDto">Supplier transfer object</param>
        /// <returns></returns>
        [Route("{id:int}")]
        public async Task<IHttpActionResult> PutSupplier(int id, SupplierDto supplierDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != supplierDto.Id)
            {
                return BadRequest();
            }
            var supplier = Mapper.Map<SupplierDto, Supplier>(supplierDto);
            await _supplierRepository.UpdateAsync(supplier);

            var suppliers = await _supplierRepository.GetAllAsync("Origin");
            var suppliersDto = Mapper.Map<IList<Supplier>, IList<SupplierDto>>(suppliers);
            // updated elasticsearch.
            await _elasticsearch.UpdateSupplierElasticSearch(suppliersDto);

            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Adds suppliers.
        /// </summary>
        /// <param name="supplierDtos">List of supplier tranfer objects</param>
        /// <response code="201">Created</response>
        /// <response code="400">Bad Request</response>
        /// <returns></returns>
        [Route("")]
        [ResponseType(typeof(SupplierCompleteDto))]
        public async Task<IHttpActionResult> PostSupplier(IList<SupplierDto> supplierDtos)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var suppliers = Mapper.Map<IList<SupplierDto>, Supplier[]>(supplierDtos);
            await _supplierRepository.AddAsync(suppliers);

            var suppliersES = await _supplierRepository.GetAllAsync("Origin");
            var suppliersDto = Mapper.Map<IList<Supplier>, IList<SupplierDto>>(suppliersES);
            // updated elasticsearch.
            await _elasticsearch.UpdateSupplierElasticSearch(suppliersDto);

            var response = new SupplierCompleteDto() { Suppliers = new List<SupplierDto>() };
            response.Suppliers = supplierDtos;

            return CreatedAtRoute("DefaultApi", new {controller = "others"}, response);
        }

        /// <summary>
        /// Delets supplier.
        /// </summary>
        /// <response code="200">OK</response>
        /// <response code="400">Bad Request</response>
        /// <response code="404">Not Found</response>
        /// <param name="id"></param>
        /// <returns></returns>
        [Route("{id}")]
        [ResponseType(typeof(SupplierCompleteDto))]
        public async Task<IHttpActionResult> DeleteSupplier(int id)
        {
           var supplier = await _supplierRepository.GetSingleAsync(s => s.Id == id);
            if (supplier == null)
            {
                return NotFound();
            }

            try
            {
                await _supplierRepository.RemoveAsync(supplier);

                var suppliers = await _supplierRepository.GetAllAsync("Origin");
                var suppliersDto = Mapper.Map<IList<Supplier>, IList<SupplierDto>>(suppliers);
                // updated elasticsearch.
                await _elasticsearch.UpdateSupplierElasticSearch(suppliersDto);
            }
            catch (DbUpdateException dbUpdateException)
            {
                 return BadRequest(dbUpdateException.InnerException.InnerException.Message.ToString());    
            } 
            var response = new SupplierCompleteDto() { Suppliers = new List<SupplierDto>() };
            response.Suppliers.Add(Mapper.Map<Supplier, SupplierDto>(supplier));
            return Ok(response);
        }


        [Route("es")]
        [HttpGet]
        public async Task<IHttpActionResult> UpdateSupplierElasticSearch()
        {
            var suppliers = await _supplierRepository.GetAllAsync("Origin");
            var suppliersDto = Mapper.Map<IList<Supplier>, IList<SupplierDto>>(suppliers);
            // updated elasticsearch.
            await _elasticsearch.UpdateSupplierElasticSearch(suppliersDto);
            return Ok();
        }

        [HttpGet]
        [Route("")]
        public async Task<SupplierCompleteDto> GetSuppliersBySearch(string query, int from = 0, int size = 20)
        {
            var supplierDto = await _elasticsearch.SearchSuppliers(query,from,size);

            var result = new SupplierCompleteDto();
            result.Suppliers = supplierDto.ToList();
            return result;
        }
    }
}