﻿using Newtonsoft.Json;

namespace PoliFemoBackend.Source.Objects.Article.News;

[Serializable]
[JsonObject(MemberSerialization.Fields)]
public class NewsPolimi
{
    private readonly string? _imgUrl;
    private readonly bool _internalNews;
    private readonly string? _subtitle;
    private readonly string? _tag;
    private readonly string? _title;
    private readonly string? _url;
    private List<string>? _content; //list of html objects (as strings)

    public NewsPolimi()
    {
    }

    public NewsPolimi(bool internalNews, string url, string title, string subtitle, string tag, string imgUrl)
    {
        _internalNews = internalNews;
        _url = url;
        _title = title;
        _subtitle = subtitle;
        _tag = tag;
        _imgUrl = imgUrl;
    }


    public string? GetUrl()
    {
        return _url;
    }

    public void SetContent(List<string> list)
    {
        _content = list;
    }

    public string? GetTitle()
    {
        return _title;
    }

    public string? GetSubtitle()
    {
        return _subtitle;
    }

    public string? GetTag()
    {
        return _tag;
    }

    public string? GetImgUrl()
    {
        return _imgUrl;
    }

    public string? GetContentAsTextJson()
    {
        if (_content == null)
            return null;

        var json = JsonConvert.SerializeObject(Utils.ArticleUtil.EncodeStringList(this._content));
        return json.Trim();
    }

    public bool IsContentEmpty()
    {
        return _content == null || _content.Count == 0 || _content.All(string.IsNullOrEmpty);
    }

    public List<string>? GetContentAsList()
    {
        return _content;
    }

    public void FixContent()
    {
        if (_content == null)
            return;

        for (var i = 0; i < _content.Count; i++)
        {
            var x = _content[i];
            x = x.Replace("\n", "<br>");
            x = x.Replace("<br>", "<br />");
            x = x.Replace("<br /><br />", "<br />");
            _content[i] = x;
        }
    }
}