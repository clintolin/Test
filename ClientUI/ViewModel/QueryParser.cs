// QueryParser.cs
//

using jQueryApi;
using KnockoutApi;
using Slick;
using SparkleXrm.GridEditor;
using System;
using System.Collections.Generic;
using System.Html;
using System.Runtime.CompilerServices;
using Xrm.Sdk;
using Xrm.Sdk.Messages;
using Xrm.Sdk.Metadata;
using Xrm.Sdk.Metadata.Query;

namespace ClientUI.ViewModels
{
    public class QueryParser
    {
        public Dictionary<string, EntityQuery> EntityLookup = new Dictionary<string, EntityQuery>();
        Dictionary<string, EntityQuery> AliasEntityLookup = new Dictionary<string, EntityQuery>();
        Dictionary<string, AttributeQuery> LookupAttributes = new Dictionary<string, AttributeQuery>();

        public QueryParser()
        {
        }

        public void QueryMetadata()
        {
            // Load the display Names
            MetadataQueryBuilder builder = new MetadataQueryBuilder();
            List<string> entities = new List<string>();
            List<string> attributes = new List<string>();

            foreach (string entityLogicalName in EntityLookup.Keys)
            {
                entities.Add(entityLogicalName);
                EntityQuery entity = EntityLookup[entityLogicalName];
                foreach (string attributeLogicalName in entity.Attributes.Keys)
                {
                    AttributeQuery attribute = entity.Attributes[attributeLogicalName];
                    string fieldName = attribute.LogicalName;
                    int pos = fieldName.IndexOf('.');
                    if (entity.AliasName != null && pos > -1)
                    {
                        fieldName = fieldName.Substr(pos);
                    }
                    attributes.Add(fieldName);
                }
            }
            builder.AddEntities(entities, new List<string>("Attributes", "DisplayName", "DisplayCollectionName", "PrimaryImageAttribute"));
            builder.AddAttributes(attributes, new List<string>("DisplayName", "AttributeType", "IsPrimaryName"));
            builder.SetLanguage((int)Script.Literal("USER_LANGUAGE_CODE"));

            RetrieveMetadataChangesResponse response = (RetrieveMetadataChangesResponse)OrganizationServiceProxy.Execute(builder.Request);
            // Update the display names
            // TODO: Add the lookup relationship in brackets for alias entitie
            foreach (EntityMetadata entityMetadata in response.EntityMetadata)
            {
                // Get the entity
                EntityQuery entityQuery = EntityLookup[entityMetadata.LogicalName];
                entityQuery.DisplayName = entityMetadata.DisplayName.UserLocalizedLabel.Label;
                entityQuery.DisplayCollectionName = entityMetadata.DisplayCollectionName.UserLocalizedLabel.Label;
                entityQuery.PrimaryImageAttribute = entityMetadata.PrimaryImageAttribute;
                entityQuery.EntityTypeCode = entityMetadata.ObjectTypeCode;
                foreach (AttributeMetadata attribute in entityMetadata.Attributes)
                {
                    if (entityQuery.Attributes.ContainsKey(attribute.LogicalName))
                    {
                        // Set the type
                        AttributeQuery attributeQuery = entityQuery.Attributes[attribute.LogicalName];
                        attributeQuery.AttributeType = attribute.AttributeType;
                        switch (attribute.AttributeType)
                        {
                            case AttributeTypeCode.Lookup:
                            case AttributeTypeCode.Picklist:
                            case AttributeTypeCode.Customer:
                            case AttributeTypeCode.Owner:
                            case AttributeTypeCode.Status:
                            case AttributeTypeCode.State:
                            case AttributeTypeCode.Boolean_:
                                LookupAttributes[attribute.LogicalName] = attributeQuery;
                                break;
                        }

                        attributeQuery.IsPrimaryName = attribute.IsPrimaryName;

                        // If the type is a lookup, then add the 'name' on to the end in the fetchxml
                        // this is so that we search the text value and not the numeric/guid value
                        foreach (Column col in attributeQuery.Columns)
                        {
                            col.Name = attribute.DisplayName.UserLocalizedLabel.Label;
                            col.DataType = attribute.IsPrimaryName.Value ? "PrimaryNameLookup" : attribute.AttributeType.ToString();
                        }
                    }
                }
            }

        }
    }

    [Imported]
    [ScriptName("Object")]
    [IgnoreNamespace]
    public class FetchQuerySettings
    {
        public string DisplayName;
        public List<Column> Columns;
        public EntityDataViewModel DataView;
        public jQueryObject FetchXml;
        public EntityQuery RootEntity;
        public Observable<string> RecordCount;
        public string OrderByAttribute;
        public bool OrderByDesending;
    }

    [Imported]
    [ScriptName("Object")]
    [IgnoreNamespace]
    public class EntityQuery
    {
        public string DisplayCollectionName;
        public string DisplayName;
        public string LogicalName;
        public string AliasName;
        public FetchQuerySettings QuickFindQuery;
        public Dictionary<string, FetchQuerySettings> Views;
        public int? EntityTypeCode;
        public string PrimaryImageAttribute;
        public Dictionary<string, AttributeQuery> Attributes;
    }

    [Imported]
    [ScriptName("Object")]
    [IgnoreNamespace]
    public class AttributeQuery
    {
        public List<Column> Columns;
        public string LogicalName;
        public AttributeTypeCode AttributeType;
        public bool? IsPrimaryName;
    }
}
