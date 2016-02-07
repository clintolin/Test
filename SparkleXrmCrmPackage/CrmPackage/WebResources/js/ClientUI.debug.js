//! ClientUI.debug.js
//

(function($){

Type.registerNamespace('ClientUI');

////////////////////////////////////////////////////////////////////////////////
// ResourceStrings

ResourceStrings = function ResourceStrings() {
}


Type.registerNamespace('ClientUI.ViewModel');

////////////////////////////////////////////////////////////////////////////////
// ClientUI.ViewModel.ObservableTestCase

ClientUI.ViewModel.ObservableTestCase = function ClientUI_ViewModel_ObservableTestCase() {
    this.AddNewVisible = ko.observable(false);
    this.productid = ko.observable();
    this.requirementdocument = ko.observable();
    ClientUI.ViewModel.ObservableTestCase.initializeBase(this);
}
ClientUI.ViewModel.ObservableTestCase.prototype = {
    _queryParser$1: null,
    
    RecordSearchCommand: function ClientUI_ViewModel_ObservableTestCase$RecordSearchCommand(term, callback) {
        if (this._queryParser$1 == null) {
            this._queryParser$1 = new ClientUI.ViewModels.QueryParser();
            this._queryParser$1.queryMetadata();
        }
        var resultsBack = 0;
        var mergedEntities = [];
        var result = ss.Delegate.create(this, function(fetchResult) {
            resultsBack++;
            var config = this._queryParser$1.entityLookup[fetchResult.get_entityName()].quickFindQuery;
            var $enum1 = ss.IEnumerator.getEnumerator(fetchResult.get_entities());
            while ($enum1.moveNext()) {
                var row = $enum1.current;
                var entityRow = row;
                var columnCount = (config.columns.length < 3) ? config.columns.length : 3;
                for (var i = 0; i < columnCount; i++) {
                    var aliasName = 'col' + i.toString();
                    row[aliasName] = row[config.columns[i].field];
                    entityRow.formattedValues[aliasName + 'name'] = entityRow.formattedValues[config.columns[i].field + 'name'];
                }
            }
            mergedEntities.addRange(fetchResult.get_entities().items());
            mergedEntities.sort(function(x, y) {
                return String.compare(x.getAttributeValueString('name'), y.getAttributeValueString('name'));
            });
        });
    }
}


////////////////////////////////////////////////////////////////////////////////
// ClientUI.ViewModel.TestGroupViewModel

ClientUI.ViewModel.TestGroupViewModel = function ClientUI_ViewModel_TestGroupViewModel() {
    ClientUI.ViewModel.TestGroupViewModel.initializeBase(this);
}
ClientUI.ViewModel.TestGroupViewModel.prototype = {
    
    requirementsDocumentSearchCommand: function ClientUI_ViewModel_TestGroupViewModel$requirementsDocumentSearchCommand(term, callback) {
        var fetchXml = "<fetch version='1.0' output-format='xml-platform' mapping='logical' distinct='false'>\r\n                                  <entity name='rta_requirementdocument'>\r\n                                    <attribute name='rta_documentname' />\r\n                                    <attribute name='rta_isineditmode' />\r\n                                    <attribute name='rta_iscurrentversion' />\r\n                                    <attribute name='rta_requirementdocumentid' />\r\n                                    <order attribute='rta_documentname' descending='false' />\r\n                                    <filter type='and'>\r\n                                      <condition attribute='statecode' operator='eq' value='0' />\r\n                                      <condition attribute='rta_requirementdocumentid' operator='eq' uitype='rta_requirementdocument' value='%{0}%' />\r\n                                    </filter>\r\n                                  </entity>\r\n                                </fetch>";
        fetchXml = String.format(fetchXml, Xrm.Sdk.XmlHelper.encode(term));
        Xrm.Sdk.OrganizationServiceProxy.beginRetrieveMultiple(fetchXml, function(result) {
            var fetchResult = Xrm.Sdk.OrganizationServiceProxy.endRetrieveMultiple(result, Xrm.Sdk.Entity);
            callback(fetchResult);
        });
    }
}


Type.registerNamespace('ClientUI.ViewModels');

////////////////////////////////////////////////////////////////////////////////
// ClientUI.ViewModels.QueryParser

ClientUI.ViewModels.QueryParser = function ClientUI_ViewModels_QueryParser() {
    this.entityLookup = {};
    this._aliasEntityLookup = {};
    this._lookupAttributes = {};
}
ClientUI.ViewModels.QueryParser.prototype = {
    
    queryMetadata: function ClientUI_ViewModels_QueryParser$queryMetadata() {
        var builder = new Xrm.Sdk.Metadata.Query.MetadataQueryBuilder();
        var entities = [];
        var attributes = [];
        var $enum1 = ss.IEnumerator.getEnumerator(Object.keys(this.entityLookup));
        while ($enum1.moveNext()) {
            var entityLogicalName = $enum1.current;
            entities.add(entityLogicalName);
            var entity = this.entityLookup[entityLogicalName];
            var $enum2 = ss.IEnumerator.getEnumerator(Object.keys(entity.attributes));
            while ($enum2.moveNext()) {
                var attributeLogicalName = $enum2.current;
                var attribute = entity.attributes[attributeLogicalName];
                var fieldName = attribute.logicalName;
                var pos = fieldName.indexOf('.');
                if (entity.aliasName != null && pos > -1) {
                    fieldName = fieldName.substr(pos);
                }
                attributes.add(fieldName);
            }
        }
        builder.addEntities(entities, ['Attributes', 'DisplayName', 'DisplayCollectionName', 'PrimaryImageAttribute']);
        builder.addAttributes(attributes, ['DisplayName', 'AttributeType', 'IsPrimaryName']);
        builder.setLanguage(USER_LANGUAGE_CODE);
        var response = Xrm.Sdk.OrganizationServiceProxy.execute(builder.request);
        var $enum3 = ss.IEnumerator.getEnumerator(response.entityMetadata);
        while ($enum3.moveNext()) {
            var entityMetadata = $enum3.current;
            var entityQuery = this.entityLookup[entityMetadata.logicalName];
            entityQuery.displayName = entityMetadata.displayName.userLocalizedLabel.label;
            entityQuery.displayCollectionName = entityMetadata.displayCollectionName.userLocalizedLabel.label;
            entityQuery.primaryImageAttribute = entityMetadata.primaryImageAttribute;
            entityQuery.entityTypeCode = entityMetadata.objectTypeCode;
            var $enum4 = ss.IEnumerator.getEnumerator(entityMetadata.attributes);
            while ($enum4.moveNext()) {
                var attribute = $enum4.current;
                if (Object.keyExists(entityQuery.attributes, attribute.logicalName)) {
                    var attributeQuery = entityQuery.attributes[attribute.logicalName];
                    attributeQuery.attributeType = attribute.attributeType;
                    switch (attribute.attributeType) {
                        case 'Lookup':
                        case 'Picklist':
                        case 'Customer':
                        case 'Owner':
                        case 'Status':
                        case 'State':
                        case 'Boolean':
                            this._lookupAttributes[attribute.logicalName] = attributeQuery;
                            break;
                    }
                    attributeQuery.isPrimaryName = attribute.isPrimaryName;
                    var $enum5 = ss.IEnumerator.getEnumerator(attributeQuery.columns);
                    while ($enum5.moveNext()) {
                        var col = $enum5.current;
                        col.name = attribute.displayName.userLocalizedLabel.label;
                        col.dataType = (attribute.isPrimaryName) ? 'PrimaryNameLookup' : attribute.attributeType;
                    }
                }
            }
        }
    }
}


Type.registerNamespace('ClientUI.View');

////////////////////////////////////////////////////////////////////////////////
// ClientUI.View.TestGroupView

ClientUI.View.TestGroupView = function ClientUI_View_TestGroupView() {
}
ClientUI.View.TestGroupView.Init = function ClientUI_View_TestGroupView$Init() {
}


Type.registerNamespace('ClientUI.Model');

////////////////////////////////////////////////////////////////////////////////
// ClientUI.Model.TestCase

ClientUI.Model.TestCase = function ClientUI_Model_TestCase() {
    ClientUI.Model.TestCase.initializeBase(this, [ 'rta_testcase' ]);
}
ClientUI.Model.TestCase.prototype = {
    rta_testcaseid: null,
    rta_name: null
}


ResourceStrings.registerClass('ResourceStrings');
ClientUI.ViewModel.ObservableTestCase.registerClass('ClientUI.ViewModel.ObservableTestCase', SparkleXrm.ViewModelBase);
ClientUI.ViewModel.TestGroupViewModel.registerClass('ClientUI.ViewModel.TestGroupViewModel', SparkleXrm.ViewModelBase);
ClientUI.ViewModels.QueryParser.registerClass('ClientUI.ViewModels.QueryParser');
ClientUI.View.TestGroupView.registerClass('ClientUI.View.TestGroupView');
ClientUI.Model.TestCase.registerClass('ClientUI.Model.TestCase', Xrm.Sdk.Entity);
ResourceStrings.AllAllSelectedTestCases = null;
ResourceStrings.ClearSearchCriteria = null;
ResourceStrings.ClearTestCases = null;
ResourceStrings.DefaultTesterSearch = null;
ResourceStrings.NewSearch = null;
ResourceStrings.RequirementDocumentsSearch = null;
ResourceStrings.ShowAllTestCases = null;
ResourceStrings.SubgridLabel = null;
ResourceStrings.SubjectMatterSearch = null;
ResourceStrings.TestTypeSearch = null;
ResourceStrings.ConfirmDeleteSelectedConnection = null;
ResourceStrings.ConfirmDeleteConnection = null;
ResourceStrings.RequiredMessage = null;
ResourceStrings.SaveButton = null;
ResourceStrings.CancelButton = null;
ResourceStrings.Connection_CollectionName = null;
ResourceStrings.ConnectTo = null;
ResourceStrings.Role = null;
ClientUI.Model.TestCase.logicalName = 'rta_testcase';
})(window.xrmjQuery);


