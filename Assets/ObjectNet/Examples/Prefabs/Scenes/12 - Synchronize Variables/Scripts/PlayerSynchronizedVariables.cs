using UnityEngine;

namespace com.onlineobject.objectnet.examples {

    public class PlayerSynchronizedVariables : NetworkBehaviour {

        [SerializeField]
        private Color playerColor = Color.white;

        private Renderer playerBodyRender;

        public void Start() {
            this.playerBodyRender = this.GetComponent<Renderer>();
        }

        public void Update() {
            this.playerBodyRender.material.SetColor("_BaseColor", this.playerColor);            
        }

        /**/
        //--------------------------------------------------------------------------
        // For internal tests purposes only
        //--------------------------------------------------------------------------

        public override void OnNetworkStarted() {
            Debug.Log(string.Format("LOGFILTER OnNetworkStart [{0}] GetNetworkId [{1}] IsOwner [{2}] IsLocal [{3}] PlayerId [{4}]", this.name, this.GetNetworkId(), this.IsOwner(), this.IsLocal(), this.GetNetworkElement().GetPlayerId() ));            
        }

        public void ActiveAwake() {
            Debug.Log(string.Format("LOGFILTER ActiveAwake [{0}] GetNetworkId [{1}] IsOwner [{2}] IsLocal [{3}] PlayerId [{4}]", this.name, this.GetNetworkId(), this.IsOwner(), this.IsLocal(), this.GetNetworkElement().GetPlayerId()));            
        }

        public void PassiveAwake() {
            Debug.Log(string.Format("LOGFILTER PassiveAwake [{0}] GetNetworkId [{1}] IsOwner [{2}] IsLocal [{3}] PlayerId [{4}]", this.name, this.GetNetworkId(), this.IsOwner(), this.IsLocal(), this.GetNetworkElement().GetPlayerId()));            
        }

        public void ActiveStart() {
            Debug.Log(string.Format("LOGFILTER ActiveStart [{0}] GetNetworkId [{1}] IsOwner [{2}] IsLocal [{3}] PlayerId [{4}]", this.name, this.GetNetworkId(), this.IsOwner(), this.IsLocal(), this.GetNetworkElement().GetPlayerId()));            
        }

        public void PassiveStart() {
            Debug.Log(string.Format("LOGFILTER PassiveStart [{0}] GetNetworkId [{1}] IsOwner [{2}] IsLocal [{3}] PlayerId [{4}]", this.name, this.GetNetworkId(), this.IsOwner(), this.IsLocal(), this.GetNetworkElement().GetPlayerId()));
        }
        
        //--------------------------------------------------------------------------
        // Test destory player and respaw it again
        //--------------------------------------------------------------------------

        public void ActiveUpdate() {
            if ( Input.GetKey(KeyCode.G) ) {
                if (NetworkManager.Instance().IsRunningLogic()) {
                    NetworkManager.Instance().DestroyOnClient(this.GetNetworkId());
                    NetworkManager.Instance().Enqueue(() => {
                        NetworkManager.Instance().SpawnPlayer();
                    }, 1000);
                } else {
                    NetworkManager.Instance().RequestPlayerRespawn(this.GetNetworkElement());
                }                
            }
        }
        /**/
    }

}