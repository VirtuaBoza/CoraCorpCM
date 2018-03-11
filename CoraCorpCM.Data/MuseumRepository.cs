﻿using CoraCorpCM.Domain.Models;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace CoraCorpCM.Data
{
    public class MuseumRepository : IMuseumRepository
    {
        private readonly ApplicationDbContext context;

        public MuseumRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        // Create

        public int Insert(IEntity entity)
        {
            context.Add(entity);
            return context.SaveChanges();
        }

        // Read

        public TEntity GetFirstEntity<TEntity>() where TEntity : class, IEntity
        {
            return context.Set<TEntity>().FirstOrDefault();
        }

        public TEntity GetEntity<TEntity>(int id) where TEntity : class, IEntity
        {
            return context.Set<TEntity>().SingleOrDefault(x => x.Id == id);
        }

        public IEnumerable<TMuseumEntity> GetEntities<TMuseumEntity>(Museum museum) where TMuseumEntity : class, IMuseumEntity
        {
            return context.Set<TMuseumEntity>().Where(x => x.Museum == museum).ToList();
        }

        public IEnumerable<TEntity> GetEntitiesAsNoTracking<TEntity>() where TEntity : class, IEntity
        {
            return context.Set<TEntity>().AsNoTracking().ToList();
        }

        public IEnumerable<TMuseumEntity> GetEntitiesAsNoTracking<TMuseumEntity>(Museum museum) where TMuseumEntity : class, IMuseumEntity
        {
            return context.Set<TMuseumEntity>().Where(x => x.Museum == museum).AsNoTracking().ToList();
        }

        public Museum GetMuseum(IMuseumEntity museumEntity)
        {
            context.Entry(museumEntity).Reference(x => x.Museum).Load();
            return museumEntity.Museum;
        }

        public bool EntityExists<TEntity>(int id) where TEntity : class, IEntity
        {
            return context.Set<TEntity>().Any(x => x.Id == id);
        }

        // Update

        public int Update(IEntity entity)
        {
            context.Update(entity);
            return context.SaveChanges();
        }

        // Delete

        public int Delete(IEntity entity)
        {
            context.Remove(entity);
            return context.SaveChanges();
        }

        public int Delete<TEntity>(int id) where TEntity : class, IEntity
        {
            var entity = context.Set<TEntity>().SingleOrDefault(x => x.Id == id);
            context.Set<TEntity>().Remove(entity);
            return context.SaveChanges();
        }
    }
}
