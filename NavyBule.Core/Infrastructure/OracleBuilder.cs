using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using NavyBule.Core.Cache;
using NavyBule.Core.Domain;
using NavyBule.Core.Util;

namespace NavyBule.Core.Infrastructure
{
    public enum ConditionType
    {
        And,
        Or,
        None
    }

    public enum ExpressionTypeDefault
    {
        Equal,
        NotEqual,
        Contains,
        In
    }
    static class ExpressionTypeExt
    {
        private static IDictionary<ExpressionTypeDefault, string> _map = new Dictionary<ExpressionTypeDefault, string>();

        static ExpressionTypeExt()
        {
            _map[ExpressionTypeDefault.Equal] = "=";//0
            _map[ExpressionTypeDefault.NotEqual] = "<>";//1
            _map[ExpressionTypeDefault.Contains] = "Like";//2

        }

        public static string GetText(this ExpressionTypeDefault expType)
        {
            if (!_map.ContainsKey(expType))
                throw new ArgumentException("expresion type :" + expType.ToString() + "not supported.");

            return _map[expType];
        }


    }
    public class Condition
    {
        public ConditionType ConditionType;
        public string Field;
        public string Comparison;
        public string Value;
        public int Priority;
        public string Tag;
        public Condition(ConditionType conditionType)
        {
            ConditionType = conditionType;
        }


        public Condition(ConditionType conditionType, string key, string assignment, string val)
        {
            ConditionType = conditionType;
            Field = key;
            Comparison = assignment;
            Value = val;
        }


        public override string ToString()
        {
            return ToString(false, "[", "]");
        }


        public virtual string ToString(bool surround, string left, string right)
        {
            string col = surround ? left + Field + right : Field;
            string val = string.IsNullOrEmpty(Value) ? "''" : Value;
            return $"{col} {Comparison} {val}";
        }
    }



    public interface IQuery
    {

        QueryData Data { get; set; }
        WhereBuilder WhereBuilder();
        IQuery AddCondition(string field, ExpressionTypeDefault comparison, object val, string tag,
            ConditionType conditionType = ConditionType.And, int priority = 0);


    }
    public class QueryData
    {
        public List<Condition> Conditions;

        public QueryData()
        {
            Conditions = new List<Condition>();
        }
    }
    public class Query : IQuery
    {
        public QueryData Data { get; set; } = new QueryData();
        protected Condition _lastCondition;

        public IQuery AddCondition(string field, ExpressionTypeDefault comparison, object val, string tag, ConditionType conditionType = ConditionType.And, int priority = 0)
        {
            StartNewCondition(conditionType, field);
            string sign = comparison.GetText();
            BuildCondition(sign, val, tag, priority);
            Complete();
            return this;
        }

        public  WhereBuilder WhereBuilder()
        {
            return new WhereBuilder();
    
        }

        protected virtual IQuery StartNewCondition(ConditionType conditionType, string fieldName)
        {
            if (_lastCondition != null)
            {
                Data.Conditions.Add(_lastCondition);
            }
            _lastCondition = new Condition(conditionType);
            _lastCondition.Field = fieldName;
            return this;
        }
        protected virtual IQuery BuildCondition(string comparison, object val,string tag, int priority = 0)
        {

            var valToUse = CommonUtilExt.ConvertVal(val);
            _lastCondition.Comparison = comparison;
            _lastCondition.Value = valToUse;
            _lastCondition.Priority = priority;
            _lastCondition.Tag = tag;
            return this;
        }



        protected void Complete()
        {
            if (_lastCondition != null)
            {
                Data.Conditions.Add(_lastCondition);
                _lastCondition = null;
            }

        }
       
        
    }
    public class OracleBuilder : ISqlBuilder
    {
        public IQuery Criteria { get; set; }
        public OracleBuilder(IQuery criteria)
        {
            Criteria = criteria;

        }

        #region common

        private static void InitPage(StringBuilder sb, TableEntity table, int skip, int take, string where, string returnFields, string orderBy)
        {
            if (string.IsNullOrEmpty(returnFields))
                returnFields = table.AllFields;

            if (string.IsNullOrEmpty(orderBy))
            {
                if (!string.IsNullOrEmpty(table.KeyName))
                {
                    orderBy = string.Format("ORDER BY \"{0}\"", table.KeyName);
                }
                else
                {
                    orderBy = string.Format("ORDER BY \"{0}\"", table.AllFieldList.First());
                }
            }
            sb.AppendFormat("SELECT * FROM(SELECT A.*,ROWNUM RN FROM (SELECT {0} FROM \"{1}\" {2} {3}) A  WHERE ROWNUM <= {4}) WHERE RN > {5}", returnFields, table.TableName, where, orderBy, skip + take, skip);
        }

        #endregion

        public string GetSchemaTableSql<T>(string returnFields)
        {
            var table = OracleCache.GetTableEntity<T>();
            if (string.IsNullOrEmpty(returnFields))
                return string.Format("SELECT {0} FROM \"{1}\" WHERE rownum=0", table.AllFields, table.TableName);
            else
                return string.Format("SELECT {0} FROM \"{1}\" WHERE rownum=0", returnFields, table.TableName);
        }

        public string GetInsertSql<T>()
        {
            return OracleCache.GetTableEntity<T>().InsertSql;
        }

        public string GetInsertReturnIdSql<T>(string sequence = null)
        {
            if (string.IsNullOrEmpty(sequence))
                throw new Exception("oracle [sequence] can't no be null or empty");
            return (OracleCache.GetTableEntity<T>().InsertReturnIdSql).Replace("```seq```", sequence);
        }

        public string GetInsertIdentitySql<T>()
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            return table.InsertIdentitySql;
        }
        public string GetUpdateSql<T>(string updateFields)
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            if (string.IsNullOrEmpty(updateFields))
            {
                return table.UpdateSql;
            }

            return CommonUtil.CreateUpdateSql(table, updateFields, "\"", "\"", ":");
        }
        public string GetUpdateSql<T>(string updateFields, bool useProperties = false)
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            if (string.IsNullOrEmpty(updateFields) && useProperties)
            {
                updateFields = string.Join(",", table.ExceptKeyAllFieldPropertiesList);
            }
            if (string.IsNullOrEmpty(updateFields))
            {
                return table.UpdateSql;
            }

            return CommonUtil.CreateUpdateSql(table, updateFields, "", "", table.AllFieldPropertiesMap, ":");
        }

        public string GetUpdateByWhereSql<T>(string where, string updateFields)
        {
            var table = OracleCache.GetTableEntity<T>();
            return CommonUtil.CreateUpdateByWhereSql(table, where, updateFields, "\"", "\"", ":");
        }

        public string GetExistsKeySql<T>()
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            return string.Format("SELECT COUNT(1) FROM {0} WHERE {1}=:{1}", table.TableName, table.KeyName);
        }

        public string GetDeleteByIdSql<T>()
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            return table.DeleteByIdSql;
        }

        public string GetDeleteByIdsSql<T>()
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            return table.DeleteByIdsSql;
        }

        public string GetDeleteByWhereSql<T>(string where)
        {
            return GetDeleteAllSql<T>() + where;
        }

        public string GetDeleteAllSql<T>()
        {
            return OracleCache.GetTableEntity<T>().DeleteAllSql;
        }

        public string GetIdentitySql()
        {
            throw new Exception("for oracle please use [GetSequenceNext] or [GetSequenceCurrent] or use [InsertReturnId]");
        }

        public string GetSequenceCurrentSql(string sequence)
        {
            return string.Format("SELECT {0}.CURRVAL FROM DUAL", sequence);
        }

        public string GetSequenceNextSql(string sequence)
        {
            return string.Format("SELECT {0}.NEXTVAL FROM DUAL", sequence);
        }

        public string GetTotalSql<T>(string where)
        {
            var table = OracleCache.GetTableEntity<T>();
            return string.Format("SELECT COUNT(1) FROM \"{0}\" {1}", table.TableName, where);
        }

        public string GetAllSql<T>(string returnFields, string orderBy)
        {
            var table = OracleCache.GetTableEntity<T>();
            if (string.IsNullOrEmpty(returnFields))
                return table.GetAllSql + orderBy;
            else
                return string.Format("SELECT {0} FROM {1} {2}", returnFields, table.TableName, orderBy);
        }

        public string GetByIdSql<T>(string returnFields)
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            if (string.IsNullOrEmpty(returnFields))
                return table.GetByIdSql;
            else
                return string.Format("SELECT {0} FROM \"{1}\" WHERE \"{2}\"=:id", returnFields, table.TableName, table.KeyName);
        }

        public string GetByIdsSql<T>(string returnFields)
        {
            var table = OracleCache.GetTableEntity<T>();
            CommonUtil.CheckTableKey(table);
            if (string.IsNullOrEmpty(returnFields))
                return table.GetByIdsSql;
            else
                return string.Format("SELECT {0} FROM \"{1}\" WHERE \"{2}\" IN :ids", returnFields, table.TableName, table.KeyName);
        }

        public string GetByIdsWithFieldSql<T>(string field, string returnFields)
        {
            var table = OracleCache.GetTableEntity<T>();
            if (string.IsNullOrEmpty(returnFields))
                returnFields = table.AllFields;
            return string.Format("SELECT {0} FROM \"{1}\" WHERE \"{2}\" IN :ids", returnFields, table.TableName, field);
        }

        public string GetByWhereSql<T>(string where, string returnFields, string orderBy)
        {
            var table = OracleCache.GetTableEntity<T>();
            if (string.IsNullOrEmpty(returnFields))
                returnFields = table.AllFields;
            return string.Format("SELECT {0} FROM {1} {2} {3}", returnFields, table.TableName, where, orderBy);
        }

        public string GetByWhereFirstSql<T>(string where, string returnFields)
        {
            var table = OracleCache.GetTableEntity<T>();
            if (string.IsNullOrEmpty(returnFields))
                returnFields = table.AllFields;
            if (!string.IsNullOrEmpty(where))
                where += " AND rownum=1";
            else
                where = "WHERE rownum=1";
            return string.Format("SELECT {0} FROM \"{1}\" {2}", returnFields, table.TableName, where);
        }

        public string GetBySkipTakeSql<T>(int skip, int take, string where, string returnFields, string orderBy)
        {
            var table = OracleCache.GetTableEntity<T>();
            StringBuilder sb = new StringBuilder();
            InitPage(sb, table, skip, take, where, returnFields, orderBy);
            return sb.ToString();
        }

        public string GetByPageIndexSql<T>(int pageIndex, int pageSize, string where, string returnFields, string orderBy)
        {
            int skip = 0;
            if (pageIndex > 0)
            {
                skip = (pageIndex - 1) * pageSize;
            }
            return GetBySkipTakeSql<T>(skip, pageSize, where, returnFields, orderBy);
        }

        public string GetPageSql<T>(int pageIndex, int pageSize, string where, string returnFields, string orderBy)
        {
            throw new Exception("for oracle please use [GetPageForOracle]");
        }

        public string BuildConditions<T>(Expression<Func<T, bool>> exp, bool includeWhere=true)
        {
            if (includeWhere)
                return " where " +   Criteria.WhereBuilder().ToSql(exp); ;

            return  Criteria.WhereBuilder().ToSql(exp); ;
           
        }

        /// <summary>
        /// Builds the WHERE conditions. Eg: where age=13 and name='kevin'
        /// </summary>
        /// <param name="includeWhere">if set to <c>true</c> [include where].</param>
        /// <returns>System.String.</returns>
        public string BuildConditions(bool includeWhere)
        {
            QueryData data = Criteria.Data;
            if (data.Conditions == null || data.Conditions.Count == 0)
                return string.Empty;

            var buffer2 = "";
            var buffer3 = new StringBuilder();
            var conditionGroups = data.Conditions.GroupBy(x => x.Priority).OrderByDescending(x => x.Key);
            var conditionGroupsList = conditionGroups.ToList();
            foreach (var conditionGroup in conditionGroupsList)
            {
                var conditions = conditionGroup.ToList();
              
                var lastConditionType = "";
                var buffer = new StringBuilder();

                for (int j = 0; j < conditions.Count; j++)
                {
                    var condition = conditions[j];
                    if ((condition.ConditionType != ConditionType.None && j > 0))
                    {
                        buffer.Append(condition.ConditionType.ToString() + " ");
                    }

                    if (j == 0 && conditionGroup.Any() && conditionGroupsList.Last() != conditionGroup)
                    {
                        lastConditionType = condition.ConditionType.ToString();
                    }

                    buffer.Append(condition.ToString() + " ");
                }

                if (conditionGroupsList.Last() == conditionGroup)
                {
                    buffer3.Append(buffer.ToString() + buffer2.ToString());
                }
                else
                {
                    var lastSegmentBuffer = lastConditionType + " ( " + buffer.ToString() + " ) ";
                    buffer2 += lastSegmentBuffer;
                    lastSegmentBuffer = "";
                }
            }
            #region "Type ConditionType.Or" is not supported following code 
            ////"Type ConditionType.Or" is not supported following code 
            //for (int ndx = 0; ndx < data.Conditions.Count; ndx++)
            //{
            //    var condition = data.Conditions[ndx];
            //    if (condition.ConditionType != ConditionType.None && ndx > 0)
            //    {
            //        buffer.Append(condition.ConditionType.ToString() + " ");
            //    }

            //    buffer.Append(condition.ToString() + " ");
            //} 
            #endregion

            if (includeWhere)
                return " where " + buffer3.ToString();

            return buffer3.ToString();
        }

       

      

    }
}
