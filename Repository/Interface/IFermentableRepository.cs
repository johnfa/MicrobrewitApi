﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microbrewit.Model;
using Microbrewit.Model.DTOs;

namespace Microbrewit.Repository
{
    public interface IFermentableRepository : IGenericDataRepository<Fermentable> 
    {
        //Fermentable AddFermentable(FermentablePostDto fermentablePostDto);
        //IList<Fermentable> GetFermentables();
        //Fermentable GetFermentable(int fermentableId);

        //IList<Grain> GetGrains();
        //IList<Sugar> GetSugars();
        //IList<DryExtract> GetDryExtracts();
        //IList<LiquidExtract> GetLiquidExtracts();
    }
}
