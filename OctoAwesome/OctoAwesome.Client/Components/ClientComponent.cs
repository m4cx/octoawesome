﻿using Microsoft.Xna.Framework;
using OctoAwesome.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace OctoAwesome.Client.Components
{
    internal class ClientComponent : GameComponent, IClientCallback
    {
        private Guid connectionId;

        private IClient client;

        public ActorProxy PlayerController { get;  private set; }

        public ClientComponent(OctoGame game) : base(game)
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None);

            DuplexChannelFactory<IClient> factory = new DuplexChannelFactory<IClient>(this, binding);
            EndpointAddress endpoint = new EndpointAddress("net.tcp://localhost:8888/Octo");
            client = factory.CreateChannel(endpoint);

            try
            {
                connectionId = client.Connect("test");
            }
            catch (Exception ex)
            {

            }

            PlayerController = new ActorProxy(client);
        }

        public override void Initialize()
        {
            base.Initialize();

            
        }

        protected override void Dispose(bool disposing)
        {
            if (client != null)
            {
                try
                {
                    client.Disconnect();
                }
                catch (Exception) { }
            }

            base.Dispose(disposing);
        }

        public void SetPosition(Index3 globalPosition, Vector3 blockPosition)
        {
            PlayerController.Position = new Coordinate(0, globalPosition, blockPosition);
        }

        public void Disconnect()
        {

        }
    }
}
