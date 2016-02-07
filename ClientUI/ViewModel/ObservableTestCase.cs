// ObservableTestCase.cs
//
using ClientUI.Model;
using ClientUI.ViewModels;
using KnockoutApi;
using SparkleXrm;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xrm.Sdk;

namespace ClientUI.ViewModel
{
    public class ObservableTestCase : ViewModelBase
    {
        #region Events

        //public event Action<string> OnSaveComplete;
        
        #endregion

        #region Observable Fields

        [PreserveCase]
        public Observable<bool> AddNewVisible = Knockout.Observable<bool>(false);
        [ScriptName("productid")]
        public Observable<Guid> ProductId = Knockout.Observable<Guid>();
        [ScriptName("requirementdocument")]
        public Observable<EntityReference> RequirementDocumentER = Knockout.Observable<EntityReference>(); 

        #endregion

        #region Private Fields

        private QueryParser _queryParser;

        #endregion

        #region Commands

        [PreserveCase]
        public void RecordSearchCommand(string term, Action<EntityCollection> callback)
        {
            if (_queryParser == null)
            {
                // Get the quick find metadata on first search
                _queryParser = new QueryParser();
                //_queryParser.GetQuickFinds();
                _queryParser.QueryMetadata();
            }

            // Get the option set values
            int resultsBack = 0;
            List<Entity> mergedEntities = new List<Entity>();
            Action<EntityCollection> result = delegate(EntityCollection fetchResult)
            {
                resultsBack++;
                FetchQuerySettings config = _queryParser.EntityLookup[fetchResult.EntityName].QuickFindQuery;
                // Add in the display Columns
                foreach (Dictionary<string, object> row in fetchResult.Entities)
                {
                    Entity entityRow = (Entity)(object)row;
                    int columnCount = config.Columns.Count < 3 ? config.Columns.Count : 3;
                    // Only get up to 3 columns
                    for (int i = 0; i < columnCount; i++)
                    {
                        // We use col<n> as the alias name so that we can show the correct values irrespective of the entity type
                        string aliasName = "col" + i.ToString();
                        row[aliasName] = row[config.Columns[i].Field];
                        entityRow.FormattedValues[aliasName + "name"] = entityRow.FormattedValues[config.Columns[i].Field + "name"];
                    }

                }
                // Merge in the results
                mergedEntities.AddRange((Entity[])(object)fetchResult.Entities.Items());

                mergedEntities.Sort(delegate(Entity x, Entity y)
                {
                    return string.Compare(x.GetAttributeValueString("name"), y.GetAttributeValueString("name"));
                });
                //if (resultsBack == connectToTypes.Length)
                //{
                //    EntityCollection results = new EntityCollection(mergedEntities);
                //    callback(results);
                //}
            };

            //foreach (string entity in connectToTypes)
            //{
            //    SearchRecords(term, result, entity);
            //}
        }

        #endregion Commands
    }
}
