﻿using CSharpFunctionalExtensions;
using EP.Balda.Data.Models;
using MediatR;

namespace EP.Balda.Logic.Queries
{
    public class GetCell : IRequest<Maybe<CellDb>>
    {
        public long Id { get; set; }

        public long MapId { get; set; }
    }
}