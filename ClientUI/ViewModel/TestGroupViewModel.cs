// TestGroupViewModel.cs
//

using ClientUI.Model;
using ClientUI.ViewModels;
using jQueryApi;
using KnockoutApi;
using Slick;
using SparkleXrm;
using SparkleXrm.GridEditor;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Xrm;
using Xrm.Sdk;

namespace ClientUI.ViewModel
{
    public class TestGroupViewModel : ViewModelBase
    {

        public void RequirementsDocumentSearchCommand(string term, Action<EntityCollection> callback)
        {
            string fetchXml = @"<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>
                                  <entity name='rta_requirementdocument'>
                                    <attribute name='rta_documentname' />
                                    <attribute name='rta_isineditmode' />
                                    <attribute name='rta_iscurrentversion' />
                                    <attribute name='rta_requirementdocumentid' />
                                    <order attribute='rta_documentname' descending='false' />
                                    <filter type='and'>
                                      <condition attribute='statecode' operator='eq' value='0' />
                                      <condition attribute='rta_requirementdocumentid' operator='eq' uitype='rta_requirementdocument' value='%{0}%' />
                                    </filter>
                                  </entity>
                                </fetch>";

            fetchXml = string.Format(fetchXml, XmlHelper.Encode(term));
            OrganizationServiceProxy.BeginRetrieveMultiple(fetchXml, delegate(object result)
            {
                EntityCollection fetchResult = OrganizationServiceProxy.EndRetrieveMultiple(result, typeof(Entity));
                callback(fetchResult);
            });
        }
    }
}
