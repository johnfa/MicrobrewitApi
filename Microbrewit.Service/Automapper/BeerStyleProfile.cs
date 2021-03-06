﻿using AutoMapper;
using Microbrewit.Model;
using Microbrewit.Model.DTOs;

namespace Microbrewit.Service.Automapper
{
    public class BeerStyleProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<BeerStyle, BeerStyleDto>()
                .ForMember(dto => dto.Id, conf => conf.MapFrom(rec => rec.BeerStyleId))
                .ForMember(dto => dto.Name, conf => conf.MapFrom(rec => rec.Name))
                .ForMember(dto => dto.SuperBeerStyle, conf => conf.MapFrom(rec => rec.SuperStyle))
                .ForMember(dto => dto.SubBeerStyles, conf => conf.MapFrom(rec => rec.SubStyles));

            Mapper.CreateMap<BeerStyle, DTO>()
               .ForMember(dto => dto.Name, conf => conf.MapFrom(rec => rec.Name))
                .ForMember(dto => dto.Id, conf => conf.MapFrom(rec => rec.BeerStyleId));

            Mapper.CreateMap<BeerStyleSimpleDto, BeerStyle>()
                 .ForMember(dto => dto.Name, conf => conf.MapFrom(rec => rec.Name))
                .ForMember(dto => dto.BeerStyleId, conf => conf.MapFrom(rec => rec.Id));

            Mapper.CreateMap<BeerStyle, BeerStyleSimpleDto>()
                  .ForMember(dto => dto.Name, conf => conf.MapFrom(rec => rec.Name))
                .ForMember(dto => dto.Id, conf => conf.MapFrom(rec => rec.BeerStyleId));


            Mapper.CreateMap<BeerStyleDto, BeerStyle>()
                .ForMember(dto => dto.BeerStyleId, conf => conf.MapFrom(rec => rec.Id))
                .ForMember(dto => dto.Name, conf => conf.MapFrom(rec => rec.Name))
                .ForMember(dto => dto.SuperStyleId, conf => conf.MapFrom(rec => rec.SuperBeerStyle.Id));
        }
    }
}