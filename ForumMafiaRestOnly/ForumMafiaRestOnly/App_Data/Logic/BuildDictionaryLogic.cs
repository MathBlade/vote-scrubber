using System;
using System.Collections.Generic;
using System.IO;

public static class BuildDictionaryLogic
{



	public static string[] ScrubDictionaryFile()
	{
        
        string PATH = AppDomain.CurrentDomain.BaseDirectory;
        string WORDS_TXT_PATH = PATH + "App_Data\\Data Files\\words.txt";
        
        if (File.Exists(WORDS_TXT_PATH))
        {
            return File.ReadAllLines(WORDS_TXT_PATH);
        }
        else
        {
            return null;
        }
        

    }

    public static List<String> GetAllWordsInString(string[] words, string bigString)
    {
        List<string> wordsFound = new List<String>();
        foreach(string word in words)
        {
            if (word.Length <= 2)
            {
                continue;
            }
            if (bigString.ContainsIgnoreCase(word))
            {
                wordsFound.Add(word.ToLower());
            }
        }
        return wordsFound;
    }




}
