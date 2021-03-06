﻿using System;

namespace FreeSql.DataAnnotations
{
    public class ColumnFluent
    {

        public ColumnFluent(ColumnAttribute column)
        {
            _column = column;
        }

        ColumnAttribute _column;
        /// <summary>
        /// 数据库列名
        /// </summary>
        public ColumnFluent Name(string value)
        {
            _column.Name = value;
            return this;
        }
        /// <summary>
        /// 指定数据库旧的列名，修改实体属性命名时，同时设置此参数为修改之前的值，CodeFirst才可以正确修改数据库字段；否则将视为【新增字段】
        /// </summary>
        public ColumnFluent OldName(string value)
        {
            _column.OldName = value;
            return this;
        }
        /// <summary>
        /// 数据库类型，如： varchar(255)
        /// </summary>
        public ColumnFluent DbType(string value)
        {
            _column.DbType = value;
            return this;
        }

        /// <summary>
        /// 主键
        /// </summary>
        public ColumnFluent IsPrimary(bool value)
        {
            _column.IsPrimary = value;
            return this;
        }
        /// <summary>
        /// 自增标识
        /// </summary>
        public ColumnFluent IsIdentity(bool value)
        {
            _column.IsIdentity = value;
            return this;
        }
        /// <summary>
        /// 是否可DBNull
        /// </summary>
        public ColumnFluent IsNullable(bool value)
        {
            _column.IsNullable = value;
            return this;
        }
        /// <summary>
        /// 忽略此列，不迁移、不插入
        /// </summary>
        public ColumnFluent IsIgnore(bool value)
        {
            _column.IsIgnore = value;
            return this;
        }
        /// <summary>
        /// 乐观锁
        /// </summary>
        public ColumnFluent IsVersion(bool value)
        {
            _column.IsVersion = value;
            return this;
        }
        /// <summary>
        /// 唯一键，在多个属性指定相同的标识，代表联合键；可使用逗号分割多个 UniqueKey 名。
        /// </summary>
        /// <param name="value">标识</param>
        /// <returns></returns>
        public ColumnFluent Unique(string value)
        {
            _column.Unique = value;
            return this;
        }
        /// <summary>
        /// 类型映射，比如：可将 enum 属性映射成 typeof(string)
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ColumnFluent MapType(Type value)
        {
            _column.MapType = value;
            return this;
        }

        /// <summary>
        /// 创建表时字段位置，规则如下：
        /// <para></para>
        /// &gt;0时排前面
        /// <para></para>
        /// =0时排中间(默认)
        /// <para></para>
        /// &lt;0时排后面
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public ColumnFluent Position(short value)
        {
            _column.Position = value;
            return this;
        }
    }
}
