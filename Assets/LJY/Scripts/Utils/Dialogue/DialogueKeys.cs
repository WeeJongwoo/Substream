namespace Utils
{
    /// <summary>
    /// 게임 내 모든 대사 상황(Situation) 키값을 관리
    /// </summary>
    public static class DialogueKeys
    {
        /// <summary>
        /// 블랙마켓 전용
        /// </summary>
        public static class BlackMarket
        {
            public const string NPC_ID = "NPC_Name_BlackMarket";

            // 입장/퇴장 및 인사
            public const string ENTER = "Enter";
            public const string EXIT = "Exit";
            public const string GREETING_POOR = "Greeting_Poor";
            public const string GREETING_NORMAL = "Greeting_Normal";
            public const string GREETING_VIP = "Greeting_VIP";

            // 새로고침
            public const string REFRESH_SUCCESS = "Refresh_Success";
            public const string REFRESH_DISABLED = "Refresh_Disabled";
            public const string REFRESH_LOCKED = "Refresh_Locked";

            // 거래 (구매/저축/멤버십)
            public const string BUY_SUCCESS = "BuySuccess";
            public const string NOT_ENOUGH_MONEY = "NotEnoughMoney";

            public const string DEPOSIT_SUCCESS = "Deposit_Success";
            public const string WITHDRAW_SUCCESS = "Withdraw_Success";
            public const string WITHDRAW_DENIED = "Withdraw_Denied";

            public const string MEMBERSHIP_UP = "Membership_Up";
            public const string MEMBERSHIP_MAX = "Membership_Max";
        }


    }
}