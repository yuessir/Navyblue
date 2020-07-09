﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Reflection;
using NavyBule.Core.Domain;
using NavyBule.Core.Util;

namespace NavyBule.Core.Cache
{
    internal class OracleCache
    {
        /// <summary>
        /// Cache
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, TableEntity> tableDict = new ConcurrentDictionary<RuntimeTypeHandle, TableEntity>();
        private static readonly object _locker = new object();
        public static TableEntity GetTableEntity<T>()
        {
            Type t = typeof(T);
            RuntimeTypeHandle typeHandle = t.TypeHandle;
            if (!tableDict.Keys.Contains(typeHandle))
            {
                lock (_locker)
                {
                    if (!tableDict.Keys.Contains(typeHandle))
                    {
                        TableEntity table = CommonUtil.CreateTableEntity(t);
                        CommonUtil.InitTableForOracle(table);
                        tableDict[typeHandle] = table;
                    }
                }
            }

            return tableDict[typeHandle];
        }
    }
    public class PropertyCache
    {
        /// <summary>
        /// Cache
        /// </summary>
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, ArrayList> propertyDict = new ConcurrentDictionary<RuntimeTypeHandle, ArrayList>();
        private static readonly object _locker = new object();
        public static ArrayList GetDtoProperties<T>()
        {
            Type t = typeof(T);
            RuntimeTypeHandle typeHandle = t.TypeHandle;
            if (!propertyDict.Keys.Contains(typeHandle))
            {
                lock (_locker)
                {
                    if (!propertyDict.Keys.Contains(typeHandle))
                    {
                        var table = GetPropertyInfo(t);
                        propertyDict[typeHandle] = table;
                    }
                }
            }

            return propertyDict[typeHandle];
        }
        /// <summary>
        /// Gets the property information.
        /// </summary>
        /// <param name="objType">Type of the object.</param>
        /// <returns>ArrayList.</returns>
        private static ArrayList GetPropertyInfo(Type objType)
        {

            var objProperties = new ArrayList();
            foreach (PropertyInfo objProperty in objType.GetProperties())
            {
                objProperties.Add(objProperty.Name);
            }

            return objProperties;

        }
    }
}
