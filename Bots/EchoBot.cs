// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.15.2

// null許容値型を有効化
#nullable enable

using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
// TextAnalyticsを使うための呪文
using System;
using System.Net.Http;
using Microsoft.Rest;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics;
using Microsoft.Azure.CognitiveServices.Language.TextAnalytics.Models;
using System.Diagnostics;

namespace EchoBot.Bots
{
    // TextAnalyticsへ接続するためのクラス定義
    class ApiKeyServiceClientCredentials : ServiceClientCredentials
    {
        private readonly string apiKey;

        public ApiKeyServiceClientCredentials(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public override Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }
            request.Headers.Add("Ocp-Apim-Subscription-Key", this.apiKey);
            return base.ProcessHttpRequestAsync(request, cancellationToken);
        }
    }

    // センチメント分析結果クラスの定義
    class SentimentAnalysisResult
    {
        public bool IsOk { get; set; }
        public double? Score { get; set; }
        public string? ErrorMessage { get; set; }

        public SentimentAnalysisResult(bool IsOk, double? Score = null, string? ErrorMessage = null)
        {
            this.IsOk = IsOk;
            this.Score = Score;
            this.ErrorMessage = ErrorMessage;
        }
    }

    // EchoBotのメインクラス定義
    public class EchoBot : ActivityHandler
    {
        // .envに定義したKEYを取得
        private static readonly string KEY = DotNetEnv.Env.GetString("KEY");
        // .envに定義したENDPOINTを取得
        private static readonly string ENDPOINT = DotNetEnv.Env.GetString("ENDPOINT");

        static TextAnalyticsClient AuthenticateClient()
        {
            ApiKeyServiceClientCredentials credentials = new ApiKeyServiceClientCredentials(KEY);
            TextAnalyticsClient client = new TextAnalyticsClient(credentials)
            {
                Endpoint = ENDPOINT,
            };
            return client;
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            // authenticateClientを呼び出し
            TextAnalyticsClient client = AuthenticateClient();
            // センチメント分析の実行
            SentimentAnalysisResult analyticsResult = SentimentAnalysis(client, turnContext.Activity.Text);

            string replyText = $"Message: {turnContext.Activity.Text}, Score: {analyticsResult.Score}";
            await turnContext.SendActivityAsync(MessageFactory.Text(replyText, replyText), cancellationToken);
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            string welcomeText = "Hello and welcome!";
            foreach (ChannelAccount member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text(welcomeText, welcomeText), cancellationToken);
                }
            }
        }

        // Azureのセンチメント分析を実行するメソッド
        static SentimentAnalysisResult SentimentAnalysis(ITextAnalyticsClient client, string message)
        {
            try
            {
                SentimentResult result = client.Sentiment(message, "ja");
                return new SentimentAnalysisResult(IsOk: true, Score: result.Score ?? -1);
            } catch(Exception e) {
                return new SentimentAnalysisResult(IsOk: false, ErrorMessage: e.Message);
            }
            
        }
    }


    

}
