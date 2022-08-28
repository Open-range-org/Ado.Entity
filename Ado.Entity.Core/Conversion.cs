﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace Ado.Entity.Core
{
    public static class Conversion
    {

        public static T Convert<T>(this Object myobj)
        {
            Type objectType = myobj.GetType();
            Type target = typeof(T);
            var x = Activator.CreateInstance(target, false);
            var z = from source in objectType.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            var d = from source in target.GetMembers().ToList()
                    where source.MemberType == MemberTypes.Property
                    select source;
            List<MemberInfo> members = d.Where(memberInfo => d.Select(c => c.Name)
               .ToList().Contains(memberInfo.Name)).ToList();
            PropertyInfo propertyInfo;
            object value;
            foreach (var memberInfo in members)
            {
                propertyInfo = typeof(T).GetProperty(memberInfo.Name);
                if (myobj.GetType().GetProperty(memberInfo.Name) == null)
                {
                    value = memberInfo.GetType().IsValueType ? Activator.CreateInstance(memberInfo.GetType()) : null;
                }
                else
                {
                    value = myobj.GetType().GetProperty(memberInfo.Name).GetValue(myobj, null);
                    propertyInfo.SetValue(x, value, null);
                }


            }
            return (T)x;
        }
        public static List<TResult> ConvertList<TInput, TResult>(this List<TInput> myobjList)
        {
            List<TResult> list = new List<TResult>();
            myobjList.ForEach(o =>
            {
                list.Add(o.Convert<TResult>());
            });
            return list;
        }
    }
}
