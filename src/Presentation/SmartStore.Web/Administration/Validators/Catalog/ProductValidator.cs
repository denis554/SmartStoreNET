﻿using FluentValidation;
using SmartStore.Admin.Models.Catalog;
using SmartStore.Services.Localization;

namespace SmartStore.Admin.Validators.Catalog
{
    public class ProductValidator : AbstractValidator<ProductModel>
    {
        public ProductValidator(ILocalizationService localizationService)
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Fields.Name.Required"));

			// validate PAnGV
			When(x => x.BasePriceEnabled, () =>
			{
				RuleFor(x => x.BasePriceMeasureUnit).NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Variants.Fields.BasePriceMeasureUnit.Required"));
				RuleFor(x => x.BasePriceBaseAmount)
					.NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Variants.Fields.BasePriceBaseAmount.Required"))
					.GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Variants.Fields.BasePriceBaseAmount.Required"));
				RuleFor(x => x.BasePriceAmount)
					.NotEmpty().WithMessage(localizationService.GetResource("Admin.Catalog.Products.Variants.Fields.BasePriceAmount.Required"))
					.GreaterThan(0).WithMessage(localizationService.GetResource("Admin.Catalog.Products.Variants.Fields.BasePriceAmount.Required"));
			});
        }
    }
}