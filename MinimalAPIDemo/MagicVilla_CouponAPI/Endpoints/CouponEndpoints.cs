﻿using AutoMapper;
using FluentValidation;
using MagicVilla_CouponAPI.Models.DTO;
using MagicVilla_CouponAPI.Models;
using MagicVilla_CouponAPI.Repository.IRepository;
using System.Net;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_CouponAPI.Endpoints
{
    public static class CouponEndpoints
    {
        public static void ConfigureCouponEndpoints(this WebApplication app)
        {
            app.MapGet("/api/coupon", GetAllCoupons).WithName("GetCoupons")
                .Produces<IEnumerable<APIResponse>>(200).RequireAuthorization();

            app.MapGet("/api/coupon/{id:int}", GetCoupon).WithName("GetCoupon")
                .Produces<APIResponse>(200);

            app.MapPost("/api/coupon", CreateCoupon).WithName("CreateCoupon")
                .Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);

            app.MapPut("/api/coupon", UpdateCoupon).WithName("UpdateCoupon")
                .Accepts<CouponUpdateDTO>("application/json").Produces<APIResponse>(200).Produces(400); ;

            app.MapDelete("/api/coupon/{id:int}", DeleteCoupon).WithName("DeleteCoupon")
                .Produces<APIResponse>(200).Produces(400);
        }

        private async static Task<IResult> GetAllCoupons(ICouponRepository _couponRepo, ILogger<Program> _logger)
        {
            APIResponse response = new();
            _logger.Log(LogLevel.Information, "Getting all coupons");
            response.Result = await _couponRepo.GetAllAsync();
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }

        private async static Task<IResult> GetCoupon(ICouponRepository _couponRepo, int id)
        {
            APIResponse response = new();
            response.Result = await _couponRepo.GetAsync(id);
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;
            return Results.Ok(response);
        }
       
        private async static Task<IResult> CreateCoupon(ICouponRepository _couponRepo, IMapper _mapper,
                IValidator<CouponCreateDTO> _validator, [FromBody] CouponCreateDTO coupon_C_DTO)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var validationResult = await _validator.ValidateAsync(coupon_C_DTO);

            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }
            if (await _couponRepo.GetAsync(coupon_C_DTO.Name.ToLower()) != null)
            {
                response.ErrorMessages.Add("Coupon Name alredy Exists");
                return Results.BadRequest(response);
            }

            Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);

            await _couponRepo.CreateAsync(coupon);
            await _couponRepo.SaveAsync();
            CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);

            response.Result = couponDTO;
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.Created;

            return Results.Ok(response);

            //return Results.Created($"/api/coupon/{coupon.Id}",coupon);
            //return Results.CreatedAtRoute("GetCoupon",new { id=coupon.Id}, couponDTO);
        }
       
        private async static Task<IResult> UpdateCoupon(ICouponRepository _couponRepo, IMapper _mapper,
                IValidator<CouponUpdateDTO> _validator, [FromBody] CouponUpdateDTO coupon_U_DTO)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            var validationResult = await _validator.ValidateAsync(coupon_U_DTO);

            if (!validationResult.IsValid)
            {
                response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
                return Results.BadRequest(response);
            }

            await _couponRepo.UpdateAsync(_mapper.Map<Coupon>(coupon_U_DTO));
            await _couponRepo.SaveAsync();

            response.Result = _mapper.Map<CouponDTO>(await _couponRepo.GetAsync(coupon_U_DTO.Id));
            response.IsSuccess = true;
            response.StatusCode = HttpStatusCode.OK;

            return Results.Ok(response);
        }
        
        private async static Task<IResult> DeleteCoupon(ICouponRepository _couponRepo, int id)
        {
            APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

            Coupon couponFromStore = await _couponRepo.GetAsync(id);
            if (couponFromStore != null)
            {
                await _couponRepo.RemoveAsync(couponFromStore);
                await _couponRepo.SaveAsync();
                response.IsSuccess = true;
                response.StatusCode = HttpStatusCode.NoContent;
                return Results.Ok(response);
            }
            else
            {
                response.ErrorMessages.Add("Invalid Id");
                return Results.BadRequest(response);
            }
        }
    }
}
