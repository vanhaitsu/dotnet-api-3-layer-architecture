﻿using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;

namespace Services.Models.ResponseModels;

public class ResponseModel
{
    [JsonIgnore] public int Code { get; set; } = StatusCodes.Status200OK;
    public bool Status => Code >= StatusCodes.Status200OK && Code < 300;
    public string Message { get; set; } = "Success";
    public object? Data { get; set; }
}