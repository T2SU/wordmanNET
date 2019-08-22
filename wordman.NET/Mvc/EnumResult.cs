using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace wordman.Mvc
{
    public class EnumResult : ActionResult, IStatusCodeActionResult
    {
        public EnumResult(Enum @enum)
        {
            this.Enum = @enum;
        }

        public Enum Enum { get; }

        public int? StatusCode => StatusCodes.Status200OK;

        public override Task ExecuteResultAsync(ActionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var response = context.HttpContext.Response;
            response.ContentType = "text/plain";
            response.StatusCode = StatusCode.GetValueOrDefault(StatusCodes.Status500InternalServerError);
            return response.WriteAsync(Enum.ToString().ToLower());
        }
    }
}
