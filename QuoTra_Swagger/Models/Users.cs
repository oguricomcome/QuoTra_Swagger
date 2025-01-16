using Microsoft.AspNetCore.Identity;

namespace QuoTra.Models
{
    /// <summary>
    /// チャレンジアクセス用ユーザーモデル
    /// </summary>
    public class ChallengeUser
    {
        public string uuid { get; set; }
        public string deviceId { get; set; }
    }

    public class Profile
    {
        public string? name { get; set; }
        public int? age { get; set; }
    }

    public class ProfileDetail
    {
        public string? prefecture { get; set; }
        public string? comment { get; set; }
    }

    /// <summary>
    ///  ユーザー登録用ユーザーモデル
    /// </summary>
    public class RegisterUser : ChallengeUser
    {
        public string role { get; set; }
        public int tokenDurationMinutes { get; set; }
        public int refreshTimes { get; set; }
    }

    /// <summary>
    /// ログイン用ユーザーモデル
    /// </summary>
    public class LoginUser : ChallengeUser
    {
        public string encAppKey { get; set; }
    }

    /// <summary>
    /// DB アクセス用モデル　読み出し
    /// </summary>
    public class HashedLoginUser
    {
        public string uuidHash { get; set; }
        public string deviceIdHash { get; set; }
        public string appKeyHash { get; set; }
    }

    /// <summary>
    /// DB アクセス用モデル　書込み
    /// </summary>
    public class HashedRecordUser : HashedLoginUser
    {
        public string appKeySalt { get; set; }
        public string role { get; set; }
        public int tokenDurationMinutes { get; set; }
        public int refreshTimes { get; set; }

        public string refreshToken { get; set; }

        public DateTime refreshTokenExpiryTime { get; set; }

        public string appKeyIV1 { get; set; }
        public string appKeyIV2 { get; set; }

    }

    public class UserDataList
    {
        public List<SendUserDetail> sendUserDetails { get; set; } = new List<SendUserDetail>();
    }


    /// <summary>
    /// QuoTra_T_UserProfiles DB　書込み
    /// </summary>
    public class SendUserDetail
    {
        public string uid { get; set; }
        public string name { get; set; }
        public string nickName { get; set; }
        public string Icon { get; set; }
        public string roleId { get; set; }
        public string mailAddress { get; set; }
        public string salesArea { get; set; }
    }

}
