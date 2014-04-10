﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using ServiceStack.Redis;
using System.Configuration;
using Microbrewit.Model.DTOs;
using Newtonsoft.Json;
using System.Reflection;
using System.Collections;
using Microbrewit.Model;
namespace Microbrewit.Api.Automapper.CustomResolvers
{
    public class HopMashStepResolver : ValueResolver<MashStep, IList<HopStepDto>>
    {
        private static readonly string redisStore = ConfigurationManager.AppSettings["redis"];
        protected override IList<HopStepDto> ResolveCore(MashStep step)
        {
            using (var redisClient = new RedisClient(redisStore))
            {
                var hopStepDtoList = new List<HopStepDto>();
                foreach (var item in step.Hops)
                {

                    var hopStepDto = new HopStepDto()
                    {
                        HopId = item.HopId,
                        StepId = item.MashStepId,
                        Amount = item.AAAmount,
                        AAValue = item.AAValue,
                    };
                    var hopJson = redisClient.GetValueFromHash("hops", hopStepDto.HopId.ToString());
                    var hop = JsonConvert.DeserializeObject<HopDto>(hopJson);
                    hopStepDto.Name = hop.Name;
                    hopStepDto.Origin = hop.Origin;
                    hopStepDto.Flavours = hop.Flavours;
                    hopStepDto.FlavourDescription = hop.FlavourDescription;

                    var hopFormJson = redisClient.GetValueFromHash("hopforms", item.HopFormId.ToString());

                    hopStepDto.HopForm = JsonConvert.DeserializeObject<DTO>(hopFormJson);
                    hopStepDtoList.Add(hopStepDto);

                }
                return hopStepDtoList;
            }
        }
    }
}