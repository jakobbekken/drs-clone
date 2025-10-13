using Godot;
using System;
using System.Text;
using System.Text.Json;

namespace Game
{
    public partial class WebSocket : Node2D
    {
        [Export] public int _port = 6969;
        private string _ip_address = "127.0.0.1";
        private WebSocketPeer _webSocketPeer;
        private Godot.Collections.Dictionary _latestInput;


        [Signal]
        public delegate void DataReceivedEventHandler(float foot1Pos, int foot1Step, float foot2Pos, int foot2Step);

        public override void _Ready()
        {
            GD.Print("IN READY");
            _webSocketPeer = new WebSocketPeer();
            var error = _webSocketPeer.ConnectToUrl($"ws://{_ip_address}:{_port}");
            if (error != Error.Ok)
            {
                GD.Print("==== ERROR ====");
                GD.Print($"Could not bind PacketPeerUdp to designated port {_port} or IP address {_ip_address}");
                GD.Print("==== ERROR ====");
                GD.Print("");
            }
            else
            {
                GD.Print("==== SUCCESS ====");
                GD.Print($"Listening on port {_port} and IP address {_ip_address}");
            }
        }

        public override void _Process(double delta)
        {
            this._webSocketPeer.Poll();
            if (_webSocketPeer.GetReadyState() == WebSocketPeer.State.Open)
            {
                while (_webSocketPeer.GetAvailablePacketCount() > 0)
                {
                    byte[] packet = _webSocketPeer.GetPacket();
                    string rawPacketString = Encoding.UTF8.GetString(packet);
                    
                    var parseResult = Json.ParseString(rawPacketString);
                    if (parseResult.VariantType != Variant.Type.Nil)
                    {
                        try
                        {
                            this._latestInput = (Godot.Collections.Dictionary)parseResult;
                        }
                        catch (Exception error)
                        {
                            GD.Print($"ERROR: {error}");
                        }

                    }
                    else
                    {
                        GD.Print("Failed to parse JSON");
                    }
                    
                    //GD.Print(this._latestInput);
                    
                    GD.Print("==== WEB-SOCKET RECEIVED DATA =====");
                    GD.Print($"Left foot: {this._latestInput["left"]}");
                    GD.Print($"Right foot: {this._latestInput["right"]}");
                    //EmitSignal(nameof(DataReceived), this._latestInput["left"], this._latestInput["left"],
                    //    this._latestInput["right"], this._latestInput["right"]);
                }
            }
        }
    }
}
