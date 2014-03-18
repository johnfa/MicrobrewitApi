﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AutoMapper;
using Microbrewit.Model;
using Microbrewit.Api.DTOs;

namespace Microbrewit.Api.Automapper
{
    public class BrewerProfile : Profile
    {
        protected override void Configure()
        {
            Mapper.CreateMap<User,BrewerDto>()
                .ForMember(dto => dto.Id, conf => conf.MapFrom(rec => rec.Username))
                .ForMember(dto => dto.BreweryName, conf => conf.MapFrom(rec => rec.BreweryName));
        }

    }
}