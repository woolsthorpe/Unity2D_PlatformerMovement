using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Transactions;
using UnityEngine;

namespace JY.PlatformerBase
{
    public class CharacterGround : MonoBehaviour
    {
        [SerializeField] private bool onGround;
        [SerializeField] private bool onFrontWall;
        [SerializeField] private bool onBackWall;

        [Space(5)]

        [Header("Ground Check  Settings")]
        [SerializeField] private Transform groundCheckPos;
        [SerializeField] private Vector3 groundSizeOffset;

        [Space(5)]

        [Header("Wall Check  Settings")]
        [SerializeField] private Transform frontCheckPos;
        [SerializeField] private Transform backCheckPos;
        [SerializeField] private Vector3 wallSizeOffset;
        private Vector2 wallOffset;

        [Space(5)]

        [Header("Layer Mask")]
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private LayerMask wallLayer;

        [Space(5)]

        [Header("Default Option")]
        [SerializeField] private bool unify_CheckBox_Offset;

       
        void Update()
        {
         
            onGround = Physics2D.OverlapBox(groundCheckPos.position, groundSizeOffset,0,groundLayer);


            //벽 체크는 게임에 따라서 사용이 될수도 있고 안될수도 있기떄문에 예외처리한다.
            if (!CheckPositionAssigned())
                return;


            if (unify_CheckBox_Offset)
                wallOffset = new Vector3(groundSizeOffset.y,groundSizeOffset.x,groundSizeOffset.z);
            else
                wallOffset = wallSizeOffset;

            onFrontWall = Physics2D.OverlapBox(frontCheckPos.position, wallOffset, 0, wallLayer);
            onBackWall = Physics2D.OverlapBox(backCheckPos.position, wallOffset, 0, wallLayer);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = onGround ?Color.green: Color.red;
            Gizmos.DrawWireCube(groundCheckPos.position, groundSizeOffset);


            if (!CheckPositionAssigned())
                return;

            Gizmos.color = onFrontWall ? Color.green : Color.red;
            Gizmos.DrawWireCube(frontCheckPos.position, wallOffset);

            Gizmos.color = onBackWall ? Color.green : Color.red;
            Gizmos.DrawWireCube(backCheckPos.position, wallOffset);
        }

        private bool CheckPositionAssigned() { return (frontCheckPos != null || backCheckPos != null); }

        public bool GetOnGround() {  return onGround;  }
        public bool GetOnFrontWall() { return onFrontWall; }
        public bool GetOnBackWall() { return onBackWall; }
    }
}
