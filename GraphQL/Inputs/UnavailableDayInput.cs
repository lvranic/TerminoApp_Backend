using System;
using HotChocolate; // GraphQL atribute
using HotChocolate.Types; // Za input type
using Microsoft.EntityFrameworkCore;
using TerminoApp_NewBackend.Models;
using TerminoApp_NewBackend.Data;

namespace TerminoApp_NewBackend.GraphQL.Inputs
{
    public class UnavailableDayInput
    {
        public DateTime Date { get; set; }
        public string AdminId { get; set; } = default!;
    }
}