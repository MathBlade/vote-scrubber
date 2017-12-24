using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace ForumMafiaRestOnly
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IVoteCountRest
    {


        //[OperationContract]
        //[WebGet(UriTemplate = "/URL={url}|PlayerList={playerList}|ReplacementList={replacementList}|ModeratorNames={moderatorNames}|DayStartPostNumbers={dayStartList}|DeadPlayers={deadListText}|PriorVCNumber={priorVCNumber}|OptionalFlavorText={optionalFlavorText}|OptionalDeadlineText={optionalDeadlineText}|OptionalVoteOverrideData={optionalVoteOverrideData}")]
        //[WebGet(UriTemplate = "/URL={url}/PlayerList={playerList}/ReplacementList={replacementList}/ModeratorNames={moderatorNames}/DayStartPostNumbers={dayStartList}/DeadPlayers={deadListText}/PriorVCNumber={priorVCNumber}/OptionalFlavorText={optionalFlavorText}/OptionalDeadlineText={optionalDeadlineText}/OptionalVoteOverrideData={optionalVoteOverrideData}")]

        //string GetVotesDebug(string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumber, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData);


        [OperationContract]
        [WebGet(UriTemplate = "?URL={url}&start={postNumber}")]
        string GetCurrentVoteCountModPost(string url, int postNumber);

        [OperationContract]
        [WebGet(UriTemplate = "/APIURL={apiURL}&start={postNumber}")]
        string GetCurrentVoteCountModPostAPI(string apiURL, string postNumber);

        [OperationContract]
        [WebGet(UriTemplate = "/Votes/URL={jsonURL}&postNumber={postNumber}")]
        string GetVotesJSON(string jsonURL, string postNumber);

        [OperationContract]
        [WebGet(UriTemplate = "/VotesByPlayer/URL={jsonURL}&postNumber={postNumber}&playerName={playerName}")]
        string GetVotesJSONByPlayer(string jsonURL, string postNumber, string playerName);

        [OperationContract]
        [WebGet(UriTemplate = "/Histories/URL={jsonURL}&postNumber={postNumber}&allHistories={allHistories}")]
        string GetHistories(string jsonURL, string postNumber, string allHistories);


        [OperationContract]
        [WebGet(UriTemplate = "/Ping")]
        string Ping();

        
        [OperationContract]
        [WebGet(UriTemplate = "/VC/URL={url}/PlayerList={playerList}/ReplacementList={replacementList}/ModeratorNames={moderatorNames}/DayStartPostNumbers={dayStartList}/DeadPlayers={deadListText}/PriorVCNumber={priorVCNumberInput}/OptionalFlavorText={optionalFlavorText}/OptionalDeadlineText={optionalDeadlineText}/OptionalVoteOverrideData={optionalVoteOverrideData}/AlphabeticalSort={alphabeticalSort}/Simple={simple}/Lsort={lSort}/CleanDay={cleanDay}/DisplayAllVCs={displayAllVCs}")]
        string GetCurrentVoteCountREST(string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData, string alphabeticalSort, string simple, string lSort, string cleanDay, string displayAllVCs);
        

       

        
        [OperationContract]
        [WebGet(UriTemplate = "/History/URL={url}/PlayerList={playerList}/ReplacementList={replacementList}/ModeratorNames={moderatorNames}/DayStartPostNumbers={dayStartList}/DeadPlayers={deadListText}/PriorVCNumber={priorVCNumberInput}/OptionalFlavorText={optionalFlavorText}/OptionalDeadlineText={optionalDeadlineText}/OptionalVoteOverrideData={optionalVoteOverrideData}/AlphabeticalSort={alphabeticalSort}/Simple={simple}/Lsort={lSort}")]
        string GetASpecificWagonHistoryREST(string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData, string alphabeticalSort, string simple, string lSort);
        


        
        [OperationContract]
        [WebGet(UriTemplate = "/Histories/URL={url}/PlayerList={playerList}/ReplacementList={replacementList}/ModeratorNames={moderatorNames}/DayStartPostNumbers={dayStartList}/DeadPlayers={deadListText}/PriorVCNumber={priorVCNumberInput}/OptionalFlavorText={optionalFlavorText}/OptionalDeadlineText={optionalDeadlineText}/OptionalVoteOverrideData={optionalVoteOverrideData}")]
        string GetAllHistoriesREST(string url, string playerList, string replacementList, string moderatorNames, string dayStartList, string deadListText, string priorVCNumberInput, string optionalFlavorText, string optionalDeadlineText, string optionalVoteOverrideData);
        



      



    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    /*[DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }*/
}
