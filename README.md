# DuckDuckGo Chat API & Client
An unofficial API wrapper for duck.ai written in C# <br />

![image](https://github.com/user-attachments/assets/f75e9a56-dc7f-48db-b045-069316ceb720)

<br />


## Disclaimer

> [!WARNING]
> This API is a proof of concept. I do not suggest you to use it, nor am i responsible for any misuse of it.
> Using this is strictly against the ToS of DuckDuckGo. See [DuckDuckGo Terms of Use](https://duckduckgo.com/duckai/privacy-terms)
<br />

---

# Features

- [x] Chat history
- [x] Markdown renderer
- [x] Default prompt
- [x] Ability to choose model
- [x] Keyword based context (such as time)

---

# How it works

The concept is really easy. First a request is sent to the `/status` endpoint. This request holds the `X-Vqd-Accept: 1` header, which tells DuckDuckGo to give us a token. The token will be returned in the requests response headers. From here on, you can send requests to the `/chat` endpoint and get the response as long as you include the token header in each request.

---

## Why ?

I had an internship for which i had to make some C# applications. This is what i came up with since i use duck.ai relatively frequently.
Gave my best to document the code as best as i could.
