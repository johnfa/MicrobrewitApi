﻿using System.Collections.Generic;
using AutoMapper;
using Microbrewit.Model;
using Microbrewit.Model.DTOs;
using Microbrewit.Repository;
using Microbrewit.Repository.Repository;
using Microbrewit.Service.Elasticsearch;
using Microbrewit.Service.Elasticsearch.Component;
using Microbrewit.Service.Elasticsearch.Interface;

namespace Microbrewit.Service.Automapper.CustomResolvers
{
    public class OtherBoilStepResolver : ValueResolver<BoilStep, IList<OtherStepDto>>
    {
        private IOtherElasticsearch _otherElasticsearch = new OtherElasticsearch();
        private IOtherRepository _otherRepository = new OtherDapperRepository();

        protected override IList<OtherStepDto> ResolveCore(BoilStep step)
        {
            var otherStepDtoList = new List<OtherStepDto>();
            foreach (var item in step.Others)
            {

                var otherStepDto = new OtherStepDto()
                {
                    OtherId = item.OtherId,
                    Amount = item.Amount,
                    RecipeId = item.RecipeId,
                };
                var other = _otherElasticsearch.GetSingle(item.OtherId);
                if (other == null)
                {
                    other = Mapper.Map<Other, OtherDto>(_otherRepository.GetSingle(item.OtherId));
                }
                otherStepDto.Name = other.Name;
                otherStepDto.Type = other.Type;
                otherStepDtoList.Add(otherStepDto);
            }
            return otherStepDtoList;

        }
    }
}