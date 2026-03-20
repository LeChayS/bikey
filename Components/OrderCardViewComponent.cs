using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using bikey.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace bikey.Components
{
    public class OrderCardViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke(OrderCardVM model)
        {
            return View(model);
        }
    }
}