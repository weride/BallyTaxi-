using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Quickode.BallyTaxi.Models.Filters
{
    public static class LanguageFilters
    {
        public static Language GetById(this IQueryable<Language> languagaes, int langId)
        {
            if (languagaes == null)
                return null;

            return
                (from l in languagaes
                 where l.LanguageId == langId
                 select l)
                 .SingleOrDefault();
        }
    }
}