﻿using CoraCorpCM.Application.Models;

namespace CoraCorpCM.Application.Subgenres.Commands.CreateSubgenre.Factory
{
    public interface ISubgenreFactory
    {
        Subgenre Create(string name, int museumId);
    }
}
