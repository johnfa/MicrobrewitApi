﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using StackExchange.Redis;
using System.Configuration;
using Microbrewit.Model.DTOs;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections;
using Microbrewit.Model;
namespace Microbrewit.Api.Automapper.CustomResolvers
{
    public class HopBoilStepResolver : ValueResolver<BoilStep, IList<HopStepDto>>
    {
        private static readonly string redisStore = ConfigurationManager.AppSettings["redis"];
        private static readonly ConnectionMultiplexer redis = ConnectionMultiplexer.Connect(redisStore);


        protected override IList<HopStepDto> ResolveCore(BoilStep step)
        {
            var redisClient = redis.GetDatabase();
            
                var hopStepDtoList = new List<HopStepDto>();
                foreach (var item in step.Hops)
                {

                    var hopStepDto = new HopStepDto()
                    {
                        HopId = item.HopId,
                        StepId = item.StepId,
                        Amount = item.Amount,
                        AAValue = item.AAValue,
                    };
                    var hopJson = redisClient.HashGet("hops", hopStepDto.HopId.ToString());
                    var hop = JsonConvert.DeserializeObject<HopDto>(hopJson);
                    hopStepDto.Name = hop.Name;
                    hopStepDto.Origin = hop.Origin;
                    hopStepDto.Flavours = hop.Flavours;
                    hopStepDto.FlavourDescription = hop.FlavourDescription;

                    var hopFormJson = redisClient.HashGet("hopforms", item.HopFormId.ToString());

                    hopStepDto.HopForm = JsonConvert.DeserializeObject<DTO>(hopFormJson);
                    hopStepDtoList.Add(hopStepDto);

                }
                return hopStepDtoList;
            
        }
    }
}