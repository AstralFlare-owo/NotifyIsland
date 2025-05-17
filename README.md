# NotifyIsland

用于将ClassIsland的提醒功能更好的和第三方通知服务集成。

## 使用方式

安装此插件，重启后打开 **调试菜单** （为避免同学乱调，故放于调试菜单中），在设置中调整需要修改的内容，将 **【启用服务】开关** 打开即可。

**开启服务并调试完毕后，务必在展开的菜单中设置访问密钥！！！**

## 工作原理

NotifyIsland将在启动后监听设置的接口（默认为`*:1379`），在其上启动一个HTTP服务器。

通过请求`/api/notify`接口，可以触发ClassIsland提醒。

（示例：发送一个标题为“提醒标题”持续3秒，内容为“提醒内容”持续15秒的提醒）

```
POST /api/notify HTTP/1.1
Accept: application/json
Authorization: Bearer 1145141919
Content-Type: application/json
Content-Length: 114

{
    "title": "提醒标题",
    "title_duration": 3,
    "content": "提醒内容"
    "content_duration": 15
}
```

```
HTTP/1.1 200 OK
Transfer-Encoding: chunked
Content-Type: application/json
Server: Microsoft-HTTPAPI/2.0
Date: Sun, 10 Aug 1919 11:45:14 GMT

{"success":true,"code":200,"message":"[200]已推送到ClassIsland"}
```

## TODO

- 多通道推送
  
  用于区分不同等级的提醒（紧急、重要、普通等）
- 重构

  ~毕竟代码写的太屎了~ 找时间让Gemini重构一下