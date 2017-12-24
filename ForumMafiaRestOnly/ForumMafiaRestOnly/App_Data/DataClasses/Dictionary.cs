using System;
using System.Collections.Generic;

public static class Dictionary
{

    private static DictionaryInstance _instance;

   
    static Dictionary()
    {

        _instance = Instance();
        
    }

    public static List<string> AllWordsInString(string bigString)
    {
        return Instance().GetAllWordsInString(bigString);
    }

    private static DictionaryInstance Instance()
    {
        if (_instance != null)
        {
            return _instance;
        }
        else
        {
            _instance = createNewInstance();
            return _instance;
        }

        
    }



    private static DictionaryInstance createNewInstance()
    {
        return new DictionaryInstance(BuildDictionaryLogic.ScrubDictionaryFile());
    }


    private class DictionaryInstance
    {
        string[] words;
        public DictionaryInstance(string[] _words)
        {
            words = _words;
        }

        public List<String> GetAllWordsInString(string bigString)
        {
            return BuildDictionaryLogic.GetAllWordsInString(words,bigString);
        }
    }
    
	
}


