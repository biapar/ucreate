﻿using System;
using System.Linq;
using System.Reflection;
using NicBell.UCreate.Attributes;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Core.Services;

namespace NicBell.UCreate.Sync
{
    public class MemberTypeSync : BaseTypeSync<MemberTypeAttribute>
    {
        /// <summary>
        /// Service
        /// </summary>
        public IMemberTypeService Service
        {
            get
            {
                return ApplicationContext.Current.Services.MemberTypeService;
            }
        }

        /// <summary>
        /// Syncs a list of membertypes
        /// </summary>>
        public override void SyncAll()
        {
            foreach (var memeberType in TypesToSync)
            {
                Save(memeberType);
            }
        }

        /// <summary>
        /// Saves
        /// </summary>
        /// <param name="itemType"></param>
        public void Save(Type itemType)
        {
            var memberTypes = Service.GetAll();
            var attr = Attribute.GetCustomAttributes(itemType).FirstOrDefault(x => x is MemberTypeAttribute) as MemberTypeAttribute;
            var mt = memberTypes.FirstOrDefault(x => x.Alias == itemType.Name) ?? new MemberType(-1);

            mt.Name = attr.Name;
            mt.Alias = itemType.Name;
            mt.Description = attr.Description;
            mt.Icon = attr.Icon;

            MapProperties(mt, itemType);

            //Set member specific properties
            var memberProperties = itemType.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                                .Select(x => Attribute.GetCustomAttribute(x, typeof (MemberPropertyAttribute), false))
                                .OfType<MemberPropertyAttribute>();

            foreach (var memberProperty in memberProperties)
            {
                mt.SetMemberCanEditProperty(memberProperty.Alias, memberProperty.CanEdit);
                mt.SetMemberCanViewProperty(memberProperty.Alias, memberProperty.ShowOnProfile);
            }

            Service.Save(mt);
        }
    }
}
