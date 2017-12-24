using System;
using System.Collections.Generic;
using HtmlAgilityPack;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Net;
using System.Linq;
using System.Web;
using System.Resources;
using System.Diagnostics;
using System.Threading.Tasks;

namespace System.Runtime.CompilerServices
{
    sealed class CallerMemberNameAttribute : Attribute { }
}

public static class RunScrubLogic
{

    private enum ThreadStatus { NotYetStarted, Running, Complete };
    private const string VOTE_COUNT_SPOILER_NAME = "Spoiler: VoteCount Settings";
    
    //playerList=a,b,creplacementList=dmoderatorNames=AlisaedayStartNumbers=edeadList=fdeadline=gflavor=hvoteOverrides=ialphaSort=jsimple=klSort=lcleanDay=mdisplayAllVCs=n

    public static string ScrubPostForSettings(string url,int postNumberInput)
    {
        
        url = "https://forum.mafiascum.net/viewtopic.php?" + url + "&start=" + postNumberInput;
        //return "Url: " + url + " Post Number Input: " + postNumberInput;

        Regex reg = new Regex("^p[0-9]", RegexOptions.IgnoreCase);
        HtmlDocument doc = GetHTMLDocumentFromURL(url);
        HtmlNode div = doc.DocumentNode.SelectSingleNode("//div");
        HtmlNode postbody = div.SelectSingleNode(".//div[contains(@class,'postbody')]");
        //string pbInnerText = postbody.InnerText;
        //string pbInnerHtml = postbody.InnerHtml;

        //return "Inner Text: " + pbInnerText + " Inner HTML: " + pbInnerHtml;

        HtmlNode content = postbody.SelectSingleNode(".//div[contains(@class,'content')]");
        HtmlNodeCollection spoilerTags = content.SelectNodes(".//div[contains(@class,'quotetitle')]");
        HtmlNodeCollection spoilerContentTags = content.SelectNodes(".//div[contains(@class,'quotecontent')]");

        //string displayString = "";
        int i = 0;
        //bool settingsFound = false;
        foreach (HtmlNode spoilerNode in spoilerTags)
        {
            //<b>Spoiler: VoteCount Settings</b>
            //HtmlNode vcNode = spoilerNode.SelectSingleNode("");
            HtmlNode spoilerHeaderTextNode = spoilerNode.SelectSingleNode(".//b");
            if (spoilerHeaderTextNode != null)
            {
                if(spoilerNode.SelectSingleNode(".//b").InnerText.Trim().Equals(VOTE_COUNT_SPOILER_NAME))
                {
                    //settingsFound = true;
                    break;
                   // return spoilerNode.InnerHtml;
                    
                }
                else
                {
                    i++;
                }
            }
            else
            {
                i++;
            }
            //displayString += spoilerNode.SelectSingleNode(".//b").InnerText + "|||||";
        }

        return spoilerContentTags[i].SelectSingleNode(".//div").InnerHtml.Trim();

        

    }

    



    public static string GetVotesDebug(string url, string playerList, string replacementList, string moderatorNamesInput, string dayStartList = null, string deadListText = null, string priorVCNumberInput = null, string optionalFlavorText = null, string optionalDeadlineText = null, string optionalVoteOverrideData = null)
    {

        return "URL: " + url + " playerList: " + playerList + " replacementList: " + replacementList + " moderatorNames: " + moderatorNamesInput + " dayStartList: " + dayStartList + " dead players: " + deadListText + " prior VC Number: " + priorVCNumberInput + " optionalVoteOverrideData: " + optionalVoteOverrideData;
    }

    



   

    public static void BuildVotes(ref List<Vote> votesOnThread, string validURL, List<Player> players, List<string> moderatorNames, List<Replacement> replacements, ref List<long> timings)
    {

        Stopwatch stopWatch = new Stopwatch();
        if (timings != null)
        {
            stopWatch.Start();
        }

        HtmlDocument doc = GetHTMLDocumentFromURL(validURL);

        if (timings != null)
        {
            timings.Add(stopWatch.ElapsedMilliseconds);            
            stopWatch.Stop();
            stopWatch = null;
        }

        HtmlNode pagination = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'pagination')]");

        int numberOfPages = int.MinValue;
        try
        {
            var links = pagination.SelectNodes(".//a[@href]");
            var lastLink = links[links.Count - 1];
            numberOfPages = Int32.Parse(lastLink.InnerText.Trim());
        }
        catch
        {
            numberOfPages = 1;
        }
        int urlStart = 0;
        if (validURL.Contains("&start"))
        {
            int indexOfnextAmp = validURL.IndexOf("&", validURL.IndexOf("&start=") + 1);
            if (indexOfnextAmp > -1)
            {
                indexOfnextAmp += 1;
                int indexOfStart = validURL.IndexOf("&start=");
                int startLength = "&start=".Length;
                int strLength = validURL.Length;
                int start = indexOfStart + startLength;
                int stop = indexOfnextAmp - (indexOfStart + startLength + 1);
                string substring = validURL.Substring(start, stop);
                urlStart = Int32.Parse(substring);
            }
            else {
                urlStart = Int32.Parse(validURL.Substring(validURL.IndexOf("&start=") + "&start=".Length));
            }
        }
        int postsPerPage = 25;


       
        votesOnThread = RunScrubOnADoc(doc, players, replacements, moderatorNames, timings);
        CoroutineReadAUrl(numberOfPages, urlStart, postsPerPage, validURL, votesOnThread, moderatorNames, players, replacements, timings);

                

    }

    private static void CoroutineReadAUrl(int numberOfPages, int urlStart, int postsPerPage, string validURL, List<Vote> votesOnThread, List<string> moderatorNames, List<Player> players, List<Replacement> replacements, List<long> timings)
    {
      

        List<Task> allTasks = new List<Task>();
        for (int i = 1; i <= numberOfPages; i++)
        {
            int desiredStart = (postsPerPage * i) + urlStart;
            if (desiredStart > (numberOfPages * postsPerPage))
            {

                continue;
            }
            int firstPostOnPage = (postsPerPage * i) + urlStart;

            allTasks.Add(Task.Factory.StartNew(() => HardWork(votesOnThread, postsPerPage, i, validURL, firstPostOnPage, players, replacements, moderatorNames, timings)));
            

        }

        Task.WaitAll(allTasks.ToArray(),System.Threading.Timeout.Infinite);
        return;


    }

    


    private static void HardWork(List<Vote> votesOnThread, int postsPerPage, int i, string validURL, int firstPostOnPage, List<Player> players, List<Replacement> replacements, List<string> moderatorNames, List<long> timings)
    {
        //Not thread safe. Not needed as scrubbing is proven to work.
        //int postNumberZeroIndexed = (int) Math.Floor( (double) firstPostOnPage / (double) postsPerPage);
        //pageNumbers.Add(postNumberZeroIndexed + 1);
        votesOnThread.AddRange(RunScrubOnAURL(validURL + "&start=" + firstPostOnPage, players, replacements, moderatorNames, timings));


    }

    private static List<Vote> RunScrubOnAURL(string validURL, List<Player> players, List<Replacement> replacements, List<string> moderatorNames, List<long> timings)
    {
        Stopwatch stopWatch = null;
        if (timings != null)
        {
            stopWatch = new Stopwatch();
            stopWatch.Start();
        }

        HtmlDocument doc = GetHTMLDocumentFromURL(validURL);

        if (timings != null)
        {
            timings.Add(stopWatch.ElapsedMilliseconds);
            stopWatch.Stop();
            stopWatch = null;
        }

        List<Vote> votes = RunScrubOnADoc(doc, players, replacements, moderatorNames, timings);
        doc = null;
        return votes;
    }

    private static List<Vote> RunScrubOnADoc(HtmlDocument doc, List<Player> players, List<Replacement> replacements, List<string> moderatorNames, List<long> timings)
    {

        //return validURL;
        //var web = new HtmlWeb();

        

        foreach (Replacement replacement in replacements)
        {
            doc.DocumentNode.InnerHtml = replacement.performReplacement(doc.DocumentNode.InnerHtml, players);
        }


        //string displayString = "";//"|validURL+: " + validURL +"|";
        HtmlNodeCollection divs = doc.DocumentNode.SelectNodes("//div");
        Regex reg = new Regex("^p[0-9]", RegexOptions.IgnoreCase);
        List<Vote> votesOnThisPage = new List<Vote>();



        bool isBoldVote = false;
        foreach (HtmlNode div in divs)
        {
            string id = div.GetAttributeValue("id", string.Empty);
            if (reg.IsMatch(id))
            {
                //displayString += id + "|";


                HtmlNode postProfile = div.SelectSingleNode(".//dl[contains(@class,'postprofile')]");
                string postAuthor = postProfile.SelectSingleNode(".//a").InnerHtml;
                //displayString += postAuthor;
                //We don't care about votes the mod has made.

                bool wasPostByModerator = false;
                foreach (string moderatorName in moderatorNames)
                {
                    if (postAuthor.Equals(moderatorName))
                    {
                        wasPostByModerator = true;
                        break;
                    }
                }

                if (wasPostByModerator == true)
                {
                    continue;
                }



                HtmlNode postbody = div.SelectSingleNode(".//div[contains(@class,'postbody')]");
                string pbInnerText = postbody.InnerText;
                string pbInnerHtml = postbody.InnerHtml;
                int postNumber = Int32.Parse(postbody.SelectSingleNode(".//strong").InnerHtml.Replace("#", ""));



                HtmlNode authorNode = postbody.SelectSingleNode(".//p[contains(@class,'author')]");
                HtmlNode firstURL = authorNode.SelectSingleNode(".//a");
                string hrefValue = firstURL.Attributes["href"].Value;
                string directPostLocation = hrefValue.Substring(hrefValue.IndexOf("#")).Replace("#p", "#");
                string bbCode = "[url=https://forum.mafiascum.net/viewtopic.php?p=" + directPostLocation.Replace("#", "") + directPostLocation + "]" + postNumber + "[/url]";

                string dateTextStart = authorNode.InnerText;
                dateTextStart = dateTextStart.Replace("Post&nbsp;#" + postNumber + "&nbsp;\n\t\t\t\t    \n\t\t\t\t\t(ISO)&nbsp;\n\t\t\t\t    \n\t\t\t      &raquo; ", "").Trim();

                DateTime timeOfPost;
                bool isValidDate = DateTime.TryParse(dateTextStart, out timeOfPost);
                if (!isValidDate)
                    throw new ArgumentException("Bad date provided for post: " + postNumber);


                HtmlNode content = postbody.SelectSingleNode(".//div[contains(@class,'content')]");

                var quotes = content.SelectNodes(".//blockquote");

                HtmlNode contentMinusQuotes = content;
                if (quotes != null)
                {
                    foreach (var quote in quotes)
                    {
                        try
                        {

                            quote.Remove();
                        }
                        catch
                        {
                            var dummy = 1;
                            
                        }
                    }
                }

                HtmlNodeCollection spoilerTags = contentMinusQuotes.SelectNodes(".//div[contains(@class,'quotetitle')]");
                HtmlNodeCollection spoilerContentTags = contentMinusQuotes.SelectNodes(".//div[contains(@class,'quotecontent')]");
                if (spoilerTags != null)
                {
                 
                   try
                    {
                        foreach(HtmlNode spoiler in spoilerTags)
                        {
                            spoiler.Remove();
                        }
                        foreach (HtmlNode spoilerContent in spoilerContentTags)
                        {
                            spoilerContent.Remove();
                        }
                    }
                    catch (Exception e)
                    {
                        var dummy = 1;
                    }
                }
                


                var votes = contentMinusQuotes.SelectNodes(".//span[contains(@class,'bbvote')]");

                bool unvoteOccurred = false;
                string playerName = null;
                HtmlNode lastVote = null;
                string unvoteName = "";
                if (votes != null)
                {
                    lastVote = votes[votes.Count - 1];
                    int voteIndex = 2;
                    unvoteOccurred = false;

                    while (lastVote.InnerHtml.Contains("UNVOTE"))
                    {
                        unvoteOccurred = true;
                        unvoteName = lastVote.InnerHtml;
                        if (votes.Count >= voteIndex)
                        {
                            lastVote = votes[votes.Count - voteIndex];
                            voteIndex++;
                        }
                        else {
                            lastVote = null;
                            break;
                        }
                    }
                    if ((lastVote == null) && (unvoteOccurred))
                    {
                        playerName = unvoteName;
                    }
                }
                else
                    lastVote = null;


                if (lastVote != null)
                {
                    playerName = null;
                    playerName = lastVote.InnerHtml.Replace("VOTE: ", "");
                    playerName = playerName.Replace("<br>", "");
                    playerName = playerName.Replace("<br >", "");
                }


                //Check bold tags next
                if ((lastVote == null) && (!unvoteOccurred))
                {
                    isBoldVote = false;
                    HtmlNode lastBoldVote;
                    //var votes = contentMinusQuotes.SelectNodes (".//span[contains(@class,'bbvote')]");
                    var boldTagTexts = contentMinusQuotes.SelectNodes(".//span[contains(@class,'noboldsig')]");
                    if (boldTagTexts != null)
                    {
                        foreach (HtmlNode node in boldTagTexts)
                        {
                            //UnityEngine.Debug.Log ("Bold Tag Inner HTML: " + node.InnerHtml);
                            //UnityEngine.Debug.Log ("Bold Tag Inner Text: " + node.InnerText);
                        }

                        lastBoldVote = boldTagTexts[boldTagTexts.Count - 1];
                        int boldVoteIndex = 2;
                        unvoteOccurred = false;

                        string[] splitBySemiColon = lastBoldVote.InnerHtml.Split(';');
                        lastBoldVote.InnerHtml = splitBySemiColon[splitBySemiColon.Length - 1];
                        lastBoldVote.InnerHtml =  lastBoldVote.InnerHtml.Replace("<br>", "");
                        lastBoldVote.InnerHtml =  lastBoldVote.InnerHtml.Replace("<br/>", "");
                        lastBoldVote.InnerHtml =  lastBoldVote.InnerHtml.Replace("<br >", "");
                        lastBoldVote.InnerHtml =  lastBoldVote.InnerHtml.Replace("<br />", "");
                        while (lastBoldVote.InnerHtml.ContainsIgnoreCase("UNVOTE"))
                        {
                            unvoteOccurred = true;
                            unvoteName = lastBoldVote.InnerText;
                            if (boldTagTexts.Count >= boldVoteIndex)
                            {
                                lastBoldVote = votes[votes.Count - boldVoteIndex];
                                boldVoteIndex++;
                            }
                            else {
                                lastBoldVote = null;
                                break;
                            }
                        }
                        if ((lastBoldVote == null) && (unvoteOccurred))
                        {
                            playerName = unvoteName;
                        }
                    }
                    else
                        lastBoldVote = null;

                    if ((lastBoldVote != null) && (lastBoldVote.InnerText.Trim().ContainsIgnoreCase("vote") == true) && ((lastBoldVote.InnerText.Trim().Substring(0, 4).ContainsIgnoreCase("vote")) || (lastBoldVote.InnerText.Trim().Substring(0, 6).ContainsIgnoreCase("unvote"))) && (!lastBoldVote.InnerHtml.Contains("<")))
                    {
                        playerName = null;
                        if (lastBoldVote.InnerText.Trim().ContainsIgnoreCase("Unvote"))
                        {
                            playerName = null;
                            unvoteOccurred = true;
                            //UnityEngine.Debug.Log ("Bold unvote found by: " + postAuthor + " in post : " + postNumber);
                            isBoldVote = true;
                        }
                        else {
                            playerName = lastBoldVote.InnerText.Replace("VOTE: ", "");
                            playerName = playerName.Replace("VOTE ", "");
                            playerName = playerName.Replace("vote: ", "");
                            playerName = playerName.Replace("vote ", "");
                            playerName = playerName.Replace("Vote: ", "");
                            playerName = playerName.Replace("Vote ", "");
                            playerName = playerName.Replace("<br>", "");
                            playerName = playerName.Replace("<br >", "");
                            playerName = playerName.Trim();

                        }

                    }

                    if (playerName != null)
                    {

                        //UnityEngine.Debug.Log ("Bold vote found by: " + postAuthor + " voted for " + playerName + " in post : " + postNumber);
                        isBoldVote = true;

                    }
                }



                //There's no way this is a valid vote.
                if ((playerName != null) && (playerName.Length > 25))
                {
                    playerName = null;
                }
                //displayString += ((lastVote != null) ? postNumber + "-" + playerName.Trim() + "|"  + "\n" : "");
                Player playerVoting = Player.FindPlayerByNameUserAid(players, postAuthor);
                Player playerVoted;
                if ((playerName != null) && (!moderatorNames.Contains(playerName)))
                {
                    string playerFriendlyName = Player.makeNameFriendly(playerName);
                    string wagonNoLynch = Player.makeNameFriendly(Wagon.NO_LYNCH);
                    if (playerFriendlyName.Equals(wagonNoLynch))
                        playerVoted = Wagon.NO_LYNCH_PLAYER;
                    else
                        playerVoted = Player.FindPlayerByNameUserAid(players, playerName);
                }
                else {
                    playerVoted = null;
                }

                bool abbreviationUsed = false;
                if ((playerVoted == null) && (playerName != null) && (!moderatorNames.Contains(playerName)) && (!unvoteOccurred))
                {

                    playerVoted = players.checkAbbreviation(playerName, postNumber);
                    if (playerVoted != null)
                    {
                        abbreviationUsed = true;
                    }
                }

                bool startsWithUsed = false;
                if ((playerVoted == null) && (playerName != null) && (!moderatorNames.Contains(playerName)) && (!unvoteOccurred) && (!abbreviationUsed))
                {
                    playerVoted = Player.FindPlayerByNameUserAidReplacementsLoopStartsWith(players, playerName, replacements);
                    if (playerVoted != null)
                    {
                        startsWithUsed = true;
                    }
                }


                if ((playerVoted == null) && (playerName != null) && (!moderatorNames.Contains(playerName)) && (!unvoteOccurred) && (!abbreviationUsed))
                {
                    playerVoted = Player.FindPlayerByNameUserAidReplacementsLoopStartsWith6(players, playerName, replacements);
                    if (playerVoted != null)
                    {
                        startsWithUsed = true;
                    }
                }

                bool wordInName = false;
                if ((playerVoted == null) && (playerName != null) && (!moderatorNames.Contains(playerName)) && (!unvoteOccurred) && (!abbreviationUsed) && (!startsWithUsed))
                {
                    string friendlyPlayerName = Player.makeNameFriendly(playerName);
                    List<Player> playersWithWord = new List<Player>();
                    foreach(Player player in players)
                    {
                        if (player.WordsInName != null)
                        {
                            if (!wordInName)
                            {
                                foreach (string word in player.WordsInName)
                                {
                                    if (!playersWithWord.Contains(player) && Player.makeNameFriendly(player.Name).ContainsIgnoreCase(playerName) && playerName.ContainsIgnoreCase(word))
                                    {
                                        playersWithWord.Add(player);
                                        
                                    }

                                }
                            }
                            else
                            {
                                continue;
                            }
                        }
                    }

                    if (playersWithWord.Count == 1)
                    {
                        playerVoted = playersWithWord[0];
                        wordInName = true;
                    }
                    
                }



                if ((playerVoted == null) && (playerName != null) && (!moderatorNames.Contains(playerName)) && (!unvoteOccurred) && (!abbreviationUsed) && (!startsWithUsed) && (!wordInName))
                {
                    Player closestLD = players.returnClosestPlayerLevensheinDistance(playerName);

                    if (closestLD != null)
                    {
                        try
                        {
                            if (playerVoting.Name != null)
                            {
                                
                                playerVoted = closestLD;
                            }
                            else {
                                System.Console.WriteLine("WTF NO PLAYER VOTING!");

                            }
                        }
                        catch
                        {
                           
                            throw new Exception("Something wrong in Levenshein Distance see post number: " + postNumber);
                        }
                    }
                }



                if ((lastVote != null) || (isBoldVote == true && !unvoteOccurred))
                {

                    //Player playerVoting = Player.FindPlayerByName (players, postAuthor);
                    //Player playerVoted = Player.FindPlayerByName (players,playerName);

                    if (playerVoting == null)
                    {
                       System.Console.WriteLine("Could not find author for post " + postNumber + " received: " + postAuthor);
                       
                    }
                    if (playerVoted == null)
                    {
                        //PrefabCreator.InitializeNewDefaultMessageBox ("Could not find person voted for in post " + postNumber + " person voted: " + playerName + " Skipping vote. ");

                    }
                    else  {

                        List<String> debugString = new List<string>();
                        debugString.Add(postAuthor);
                        debugString.Add(playerName);
                        debugString.Add(bbCode);
                        debugString.Add("" + postNumber);
                        debugString.Add(timeOfPost.ToString());

                        votesOnThisPage.Add(new Vote(playerVoting, playerVoted, postNumber, timeOfPost, isBoldVote, debugString));
                    }
                }
                else if (playerName != null && (playerName.ContainsIgnoreCase("UNVOTE") || unvoteOccurred == true) && playerVoting != null)
                {
                    List<String> debugString = new List<string>();
                    debugString.Add(postAuthor);
                    debugString.Add(playerName);
                    debugString.Add(bbCode);
                    debugString.Add("" + postNumber);
                    debugString.Add(timeOfPost.ToString());
                    Player unvoteTarget = new Player("UNVOTE: ");
                    votesOnThisPage.Add(new Vote(playerVoting, unvoteTarget, postNumber, timeOfPost, isBoldVote, debugString));
                }



            }
        }

        

        return votesOnThisPage;


    }



    

    public static HtmlDocument GetHTMLDocumentFromURL(string validURL)
    {
        try
        {
            ServicePointManager.ServerCertificateValidationCallback = MyRemoteCertificateValidationCallback;
            var web = new HtmlWeb();

            return web.Load(validURL, "GET");

        }
        catch
        {

            System.Console.WriteLine("An error occurred scraping the document. Try again later.");
            return null;

        }
    }

    

    private static bool MyRemoteCertificateValidationCallback(System.Object sender,
        X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
    {
        bool isOk = true;
        // If there are errors in the certificate chain,
        // look at each error to determine the cause.
        if (sslPolicyErrors != SslPolicyErrors.None)
        {
            for (int i = 0; i < chain.ChainStatus.Length; i++)
            {
                if (chain.ChainStatus[i].Status == X509ChainStatusFlags.RevocationStatusUnknown)
                {
                    continue;
                }
                chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
                chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
                chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
                bool chainIsValid = chain.Build((X509Certificate2)certificate);
                if (!chainIsValid)
                {
                    isOk = false;
                    break;
                }
            }
        }
        return isOk;
    }
}
