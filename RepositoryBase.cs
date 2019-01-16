using EntityFramework.Extensions;
using EntityFramework.Future;
using Hwapu.KydWebSite.Data.Context;
using Hwapu.KydWebSite.Data.Interface;
using Hwapu.KydWebSite.Entity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hwapu.KydWebSite.Data
{
    public abstract class RepositoryBase<T> where T : class
    {
        private KydWebSiteContext dataContext;
        private readonly IDbSet<T> dbset;
        protected RepositoryBase(IDatabaseFactory databaseFactory)
        {
            DatabaseFactory = databaseFactory;
            dbset = DataContext.Set<T>();
        }

        protected IDatabaseFactory DatabaseFactory
        {
            get;
            private set;
        }

        protected KydWebSiteContext DataContext
        {
            get { return dataContext ?? (dataContext = DatabaseFactory.Get()); }
        }
        public virtual void Add(T entity)
        {
            dbset.Add(entity);
        }
        public virtual void Update(T entity)
        {
            dbset.Attach(entity);
            dataContext.Entry(entity).State = EntityState.Modified;
        }
        public virtual void Delete(T entity)
        {
            dbset.Remove(entity);
        }
        public virtual void Delete(Expression<Func<T, bool>> where)
        {
            IEnumerable<T> objects = dbset.Where<T>(where).AsEnumerable();
            foreach (T obj in objects)
                dbset.Remove(obj);
        }
        public virtual T GetById(long id)
        {
            return dbset.Find(id);
        }
        public virtual T GetById(string id)
        {
            return dbset.Find(id);
        }
        public virtual IEnumerable<T> GetAll()
        {
            return dbset.ToList();
        }

        public virtual IEnumerable<T> GetMany(Expression<Func<T, bool>> where)
        {
            return dbset.Where(where).ToList();
        }

        public virtual IEnumerable<T> GetManyAsNoTracking(Expression<Func<T, bool>> where)
        {
            return dbset.Where(where).AsNoTracking().ToList();
        }


        public T Get(Expression<Func<T, bool>> where)
        {
            return dbset.Where(where).FirstOrDefault<T>();
        }

        /// <summary>
        /// 获取某个表的记录数量
        /// </summary>
        /// <param name="predicate"></param>
        /// <returns></returns>
        public int Count(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return dbset.Count();
            }
            return dbset.Count(predicate);
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="TKey">排序类型</typeparam>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">每页大小</param>
        /// <param name="isAsc">是否升序排列，默认升序</param>
        /// <param name="predicate">查询条件表达式</param>
        /// <param name="keySelector">排序表达式</param>
        /// <returns></returns>
        public virtual IPage<T> Page<TKey>(int pageIndex, int pageSize, Expression<Func<T, bool>> predicate,
            Expression<Func<T, TKey>> keySelector, bool isAsc = true)
        {
            if (pageIndex <= 0 && pageSize <= 0)
            {
                throw new Exception("pageIndex或pageSize不能小于等于0！");
            }
            IPage<T> page = new Page<T>()
            {
                PageIndex = pageIndex,
                PageSize = pageSize
            };
            int skip = (pageIndex - 1) * pageSize;
            if (predicate == null)
            {
                FutureCount fcount = this.dbset.FutureCount();
                FutureQuery<T> futureQuery = isAsc
                    ? this.dbset.AsNoTracking().OrderBy(keySelector).Skip(skip).Take(pageSize).Future()
                    : this.dbset.AsNoTracking().OrderByDescending(keySelector).Skip(skip).Take(pageSize).Future();
                page.TotalItems = fcount.Value;
                page.Items = futureQuery.ToList();
                page.TotalPages = page.TotalItems / pageSize;
                if ((page.TotalItems % pageSize) != 0) page.TotalPages++;
            }
            else
            {
                var queryable = this.dbset.AsNoTracking().Where(predicate);
                FutureCount fcount = queryable.FutureCount();
                FutureQuery<T> futureQuery = isAsc
                    ? queryable.OrderBy(keySelector).Skip(skip).Take(pageSize).Future()
                    : queryable.OrderByDescending(keySelector).Skip(skip).Take(pageSize).Future();
                page.TotalItems = fcount.Value;
                page.Items = futureQuery.ToList();
                page.TotalPages = page.TotalItems / pageSize;
                if ((page.TotalItems % pageSize) != 0) page.TotalPages++;
            }
            return page;
        }

       /// <summary>
       /// 部分更新
       /// </summary>
       /// <param name="entity"></param>
       /// <param name="updatedProperties"></param>
        //public virtual void UpdatePartial(T entity, params Expression<Func<T, object>>[] updatedProperties)
        //{
        //    var dbEntityEntry = dataContext.Entry(entity);
        //    if (updatedProperties.Any())
        //    {
        //        foreach (var property in updatedProperties)
        //        {
        //            dbEntityEntry.Property(property).IsModified = true;
        //        }
        //    }
        //    else
        //    {
        //        foreach (var property in dbEntityEntry.OriginalValues.PropertyNames)
        //        {
        //            var original = dbEntityEntry.OriginalValues.GetValue<object>(property);
        //            var current = dbEntityEntry.CurrentValues.GetValue<object>(property);
        //            if (original != null && !original.Equals(current))
        //            {
        //                dbEntityEntry.Property(property).IsModified = true;
        //            }
        //        }
        //    }
        //    //return dataContext.SaveChanges();
        //}


    }

}
