﻿using System;
using System.Collections.Generic;
using System.Text;
using Infrastructure.ServiceStack;
using ServiceStack;

namespace eShop.Catalog.Product.Services
{
    public class UpdateDescriptionProduct : DomainCommand
    {
        public Guid ProductId { get; set; }
        public string Description { get; set; }
    }
}