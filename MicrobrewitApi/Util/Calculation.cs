﻿using Microbrewit.Model;
using Microbrewit.Api.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using Microbrewit.Model.DTOs;
using System.Configuration;
using Microbrewit.Repository;

namespace Microbrewit.Api.Util
{
    public static class Calculation
    {
        private static Elasticsearch.ElasticSearch _elasticsearch = new Elasticsearch.ElasticSearch();
        private static IFermentableRepository _fermentableRepository = new FermentableRepository();

        public static SRM CalculateSRM(Recipe recipe)
        {
            var srm = new SRM();
            

            foreach (var mashStep in recipe.MashSteps)
            {
                foreach (var fermentable in mashStep.Fermentables)
                {
                    srm.Standard += Math.Round(Formulas.MaltColourUnits(fermentable.Amount, fermentable.Lovibond, mashStep.Volume), 0);
                    srm.Morey += Math.Round(Formulas.Morey(fermentable.Amount, fermentable.Lovibond, mashStep.Volume), 0);
                    srm.Mosher += Math.Round(Formulas.Morey(fermentable.Amount, fermentable.Lovibond, mashStep.Volume), 0);
                    srm.Daniels += Math.Round(Formulas.Daniels(fermentable.Amount, fermentable.Lovibond, mashStep.Volume), 0);
                }
            }

            return srm;
        }

        public static IBU CalculateIBU(Recipe recipe)
        {
            var og = recipe.OG;
            var ibu = new IBU();
            var tinseth = 0.0;
            var rager = 0.0;
            foreach (var boilStep in recipe.BoilSteps)
            {
                var tinsethUtilisation = Formulas.TinsethUtilisation(og, boilStep.Length);
                var ragerUtilisation = Formulas.RangerUtilisation(boilStep.Length);
                foreach (var hop in boilStep.Hops)
                {
                    var tinasethMgl = Formulas.TinsethMgl(hop.Amount, hop.AAValue, recipe.Volume);
                    tinseth += Formulas.TinsethIbu(tinasethMgl, tinsethUtilisation);
                    rager += Formulas.RangerIbu(hop.Amount, ragerUtilisation, hop.AAValue, recipe.Volume, og);
                }
            }
            ibu.Tinseth = Math.Round(tinseth, 1);
            ibu.Rager = Math.Round(rager, 1);
            return ibu;
        }

        public static double CalculateOG(Recipe recipe)
        {
            var og = 0.0;
            foreach (var mashStep in recipe.MashSteps)
            {
                foreach (var fermentable in mashStep.Fermentables)
                {
                    if (fermentable.PGG <= 0)
                    {
                        var esFermentable = _elasticsearch.GetFermentable(fermentable.FermentableId).Result;
                        if (esFermentable != null && esFermentable.PPG > 0)
                        {
                            fermentable.PGG = esFermentable.PPG;
                            //og += Formulas.MaltOG(fermentable.Amount, esFermentable.PPG, recipe.Efficiency, recipe.Volume);
                        }
                        else
                        {
                            var efFermentable = _fermentableRepository.GetSingle(f => f.Id == fermentable.FermentableId);
                            if (efFermentable != null && efFermentable.PPG != null)
                            {
                                fermentable.PGG = (int)efFermentable.PPG;
                            }
                            //og += Formulas.MaltOG(fermentable.Amount, (int)efFermentable.PPG, recipe.Efficiency, recipe.Volume);
                        }

                    }
                    og += Formulas.MaltOG(fermentable.Amount, (int)fermentable.PGG, recipe.Efficiency, recipe.Volume);
 
                }
            }
            return Math.Round(1 + og / 1000, 4);
        }

        public static ABV CalculateABV(Recipe recipe)
        {
            var abv = new ABV();
            abv.Standard = Math.Round(Formulas.MicrobrewitABV(recipe.OG, recipe.FG), 2);
            abv.Miller = Math.Round(Formulas.MillerABV(recipe.OG, recipe.FG), 2);
            abv.Simple = Math.Round(Formulas.SimpleABV(recipe.OG, recipe.FG), 2);
            abv.Advanced = Math.Round(Formulas.AdvancedABV(recipe.OG, recipe.FG), 2);
            abv.AdvancedAlternative = Math.Round(Formulas.AdvancedAlternativeABV(recipe.OG, recipe.FG), 2);
            abv.AlternativeSimple = Math.Round(Formulas.SimpleAlternativeABV(recipe.OG, recipe.FG), 2);

            return abv;
        }
    }
}