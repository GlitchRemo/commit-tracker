namespace CosmosBootstrapper;

public class BootstrapperConfiguration
{
	public IReadOnlyDictionary<string, List<string>> DatabaseToCollectionMap => _databaseToCollectionMap;
	public IReadOnlyDictionary<string, List<string>> CollectionToDocumentMap => _collectionToDocumentMap;

    private readonly Dictionary<string, List<string>> _databaseToCollectionMap = new()
    {
        { "userDb", new List<string> { "users" } },
        { "accountLinkDb", new List<string> { "accountLinks" } },
        { "billingAccountDb", new List<string> { "billingAccountDetails", "serviceAddresses" } },
    };

    private readonly Dictionary<string, List<string>> _collectionToDocumentMap = new()
    {
	    {
		    "users", new List<string>
		    {
			    @"{
				""_id"" : ObjectId(""65ce4372f53daf5a3ac79fd6""),
				""cssProfileId"" : ""fcd48f63363843c9bf96dcf773ce4366"",
				""userId"" : ""660ef6fa-fb12-4291-9d0d-3c0ca793e7d6"",
				""firstName"" : ""ffggg"",
				""lastName"" : ""dddd"",
				""organizationName"" : null,
				""isActive"" : true,
				""notificationEmail"" : ""sekhusman.ulla@nationalgrid.com"",
				""addedOn"" : {
					""$date"" : 1708016498043
				},
				""signinEmailAddress"" : ""adhoc5@gmail.com"",
				""source"" : ""UDM"",
				""published"" : false,
				""isLatest"" : true
			}"
		    }
	    },
	    {
		    "billingAccountDetails", new List<string>
		    {
			    @"{
					""_id"" : ObjectId(""65cd422008156a5cfc9645d4""),
					""number"" : ""0062109004"",
					""statusCssCode"" : ""02"",
					""status"" : ""Active"",
					""typeCssCode"" : ""C"",
					""type"" : ""NonResidential"",
					""regionCssCode"" : ""5"",
					""region"" : ""MassachusettsElec"",
					""regionAbbreviation"" : ""MA"",
					""paperlessBillingStatusCssCode"" : ""01"",
					""paperlessBillingStatus"" : ""UseDefaultOnly"",
					""activationDate"" : {
						""$date"" : 1151625600000
					},
					""closedDate"" : {
						""$date"" : 253402214400000
					},
					""isCashOnly"" : false,
					""isEnrolledInPaperlessBilling"" : true,
					""isActive"" : true,
					""premiseNumber"" : 6210900,
					""customerNumber"" : 344008849,
					""addedBy"" : ""RecurringPayUpdated"",
					""source"" : ""CSS"",
					""addedOn"" : {
						""$date"" : 1707950619672
					},
					""collectionStatusCssCode"" : ""X"",
					""collectionStatus"" : ""NotInCollections"",
					""isEnrolledInPaymentPlan"" : false,
					""isEnrolledInCollectionArrangement"" : false,
					""collectionStatusDate"" : {
						""$date"" : 1661904000000
					},
					""isEnrolledInRecurringPay"" : true,
					""recurringPaymentSequenceNumber"" : 1,
					""fuelTypes"" : [
						{
							""mptKey"" : ""1"",
							""typeCssCode"" : ""0200"",
							""type"" : ""Electric"",
							""typeAbbreviation"" : ""Elec"",
							""servicePointKey"" : ""44800823"",
							""startDate"" : {
								""$date"" : 1151625600000
							},
							""endDate"" : {
								""$date"" : 253402214400000
							},
							""isActive"" : true,
							""addedOn"" : {
								""$date"" : 1707951436450
							}
						}
					],
					""currentBalance"" : -3.56,
					""isMakeOneTimePaymentEligible"" : true,
					""isEligibleForDirectPayment"" : false,
					""isEligibleForRecurringPay"" : false,
					""isLongTermPlanEligible"" : false,
					""isEligibleForBalancedBilling"" : true,
					""outstandingAccountBalance"" : -3.56,
					""proposedBudgetAmount"" : 10,
					""balancedBillingIneligibleReason"" : null,
					""balancedBillingIneligibleStatus"" : null
				}"
		    }
	    },
	    {
		    "accountLinks", new List<string>
		    {
			    @"{
					""_id"" : ObjectId(""65ce437303684e7b20e4d934""),
					""userId"" : ""660ef6fa-fb12-4291-9d0d-3c0ca793e7d6"",
					""billingAccountId"" : ""0062109004"",
					""nickName"" : """",
					""isActive"" : true,
					""upsertBy"" : ""Internal API"",
					""source"" : ""UDM"",
					""upsertOn"" : {
						""$date"" : 1712306738083
					},
					""correlationId"" : ""db63a80d-64bc-4055-a3c0-0e55b904d73c"",
					""headers"" : ""{\""Host\"":\""account-link-api:8080\"",\""Accept-Encoding\"":\""gzip, deflate\"",\""Content-Type\"":\""application/json; charset=UTF-8\"",\""Request-Id\"":\""|30c71ba4f6b351f6366dc861a9aa9f11.fc83f1295f8c0d04.\"",\""traceparent\"":\""00-30c71ba4f6b351f6366dc861a9aa9f11-fc83f1295f8c0d04-00\"",\""Content-Length\"":\""123\"",\""operationType\"":\""Insert\"",\""correlation-id\"":\""db63a80d-64bc-4055-a3c0-0e55b904d73c\"",\""source\"":\""IL\"",\""timestamp\"":\""2024-04-05T08:45:35.2863322+00:00\"",\""is-updated-in-udm\"":\""true\"",\""ContentType\"":\""application/json\""}"",
					""accountSource"" : ""CSS""
				}"
		    }
	    }
    };
}