using System;

public class VoteScrubInformationObject
{

    string priorVCNumberInput;
    string urlOfGame;
    string playerTextInput;
    string replacementTextInput;
    string moderatorNamesInput;
    string dayNumbersInput;
    string deadListInput;
    string deadLineInput;
    string flavorInput;
    string voteOverridesInput;
    string alphaSortInput;
    string simpleInput;
    string lSortInput;
    string cleanDayInput;
    string displayAllVCsInput;

    public VoteScrubInformationObject(string _priorVCInput, string _urlOfGame, string _playerTextInput, string _dayNumbersInput, string _replacementTextInput, string _moderatorNamesInput, string _deadListInput, string _deadlineInput,string _flavorInput, string _voteOverridesInput,string _alphaSortInput,string _simpleInput,string _lSortInput,string _cleanDayInput, string _displayAllVCsInput)
	{
        priorVCNumberInput = _priorVCInput;
        urlOfGame = _urlOfGame;
        playerTextInput = _playerTextInput;
        replacementTextInput = _replacementTextInput;
        moderatorNamesInput = _moderatorNamesInput;
        dayNumbersInput = _dayNumbersInput;
        deadListInput = _deadListInput;
        deadLineInput = _deadlineInput;
        flavorInput = _flavorInput;
        voteOverridesInput = _voteOverridesInput;
        alphaSortInput = _alphaSortInput;
        simpleInput = _simpleInput;
        lSortInput = _lSortInput;
        cleanDayInput = _cleanDayInput;
        displayAllVCsInput = _displayAllVCsInput;
    }

    public string PriorVCNumberInput { get => priorVCNumberInput; set => priorVCNumberInput = value; }
    public string UrlOfGame { get => urlOfGame; set => urlOfGame = value; }
    public string PlayerTextInput { get => playerTextInput; set => playerTextInput = value; }
    public string ReplacementTextInput { get => replacementTextInput; set => replacementTextInput = value; }
    public string ModeratorNamesInput { get => moderatorNamesInput; set => moderatorNamesInput = value; }
    public string DayNumbersInput { get => dayNumbersInput; set => dayNumbersInput = value; }
    public string DeadListInput { get => deadListInput; set => deadListInput = value; }
    public string DeadLineInput { get => deadLineInput; set => deadLineInput = value; }
    public string FlavorInput { get => flavorInput; set => flavorInput = value; }
    public string VoteOverridesInput { get => voteOverridesInput; set => voteOverridesInput = value; }
    public string AlphaSortInput { get => alphaSortInput; set => alphaSortInput = value; }
    public string SimpleInput { get => simpleInput; set => simpleInput = value; }
    public string LSortInput { get => lSortInput; set => lSortInput = value; }
    public string CleanDayInput { get => cleanDayInput; set => cleanDayInput = value; }
    public string DisplayAllVCsInput { get => displayAllVCsInput; set => displayAllVCsInput = value; }
}
