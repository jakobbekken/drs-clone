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
        private float _leftX;
        private float _rightX;
        private string _leftState;
        private string _rightState;


        [Signal]
        public delegate void DataReceivedEventHandler(float leftX, float rightX, string leftState, string rightState);

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
                            Godot.Collections.Dictionary rootDict = (Godot.Collections.Dictionary)parseResult;
                            Godot.Collections.Dictionary leftDict = (Godot.Collections.Dictionary)rootDict["left"];
                            Godot.Collections.Dictionary rightDict = (Godot.Collections.Dictionary)rootDict["right"];

                            this._leftX = (float)leftDict["x"];
                            this._rightX = (float)rightDict["x"];
                            this._leftState = (string)leftDict["state"];
                            this._rightState = (string)rightDict["state"];
                            
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
                    GD.Print($"Left foot - xPos: {this._leftX}, state: {this._leftState}");
                    GD.Print($"Right foot - xPos: {this._rightX}, state: {this._rightState}");
                    EmitSignal(SignalName.DataReceived, this._leftX, this._rightX, this._leftState, this._rightState);
                }
            }
        }
    }
}
