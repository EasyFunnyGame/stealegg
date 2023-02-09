using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraSettingCanvas : BaseCanvas
{
    public Button btn_show_and_hide;

    public Button btn_near;

    public Button btn_far;

    public RectTransform content;

    public bool upper;

    public Text eulerX;

    public Text eulerY;

    public Text eulerZ;

    public Text nearHeight;

    public Text farHeight;

    public Text smoothTime;

    public Button btn_euler_x_min;
    public Slider sld_euler_x;
    public Button btn_euler_x_add;

    public Button btn_euler_y_min;
    public Slider sld_euler_y;
    public Button btn_euler_y_add;

    public Button btn_euler_z_min;
    public Slider sld_euler_z;
    public Button btn_euler_z_add;

    public Button btn_height_near_min;
    public Slider sld_height_near;
    public Button btn_height_near_add;

    public Button btn_height_far_min;
    public Slider sld_height_far;
    public Button btn_height_far_add;

    public Button btn_smooth_min;
    public Slider sld_smooth;
    public Button btn_smooth_add;

    public GameCamera m_gamecamera;

    public Player m_player;


    // Start is called before the first frame update
    void Start()
    {
        btn_show_and_hide.onClick.AddListener(ExpandOrHide);

        btn_near.onClick.AddListener(CameraNear);

        btn_far.onClick.AddListener(CameraFar);

        btn_euler_x_add.onClick.AddListener(AddEulerX); 
        sld_euler_x.onValueChanged.AddListener(OnEulerXChanged);
        btn_euler_x_min.onClick.AddListener(MinEulerX);

        btn_euler_y_add.onClick.AddListener(AddEuluerY);
        sld_euler_y.onValueChanged.AddListener(EulerYChange);
        btn_euler_y_min.onClick.AddListener(MinEulerY); 

        btn_euler_z_add.onClick.AddListener(AddEulerZ); 
        sld_euler_z.onValueChanged.AddListener(OnEulerZChanged);
        btn_euler_z_min.onClick.AddListener(MinEulerZ);

        btn_height_near_min.onClick.AddListener(MinHeightNear);
        sld_height_near.onValueChanged.AddListener(OnEHeightNearChanged);
        btn_height_near_add.onClick.AddListener(AddHeightNear);

        btn_height_far_min.onClick.AddListener(MinHeightFar);
        sld_height_far.onValueChanged.AddListener(OnEHeightFarChanged);
        btn_height_far_add.onClick.AddListener(AddHeightFar);

        btn_smooth_min.onClick.AddListener(MinSmoothTime);
        sld_smooth.onValueChanged.AddListener(onSmoothTimeChanged);
        btn_smooth_add.onClick.AddListener(AddSmoothTime);
    }

    // Update is called once per frame
    void Update()
    {
        if (!expand) return;
        if (!m_gamecamera) return;
        if (!m_player) return;

        eulerX.text = m_gamecamera.transform.eulerAngles.x.ToString();
        eulerY.text = m_gamecamera.transform.eulerAngles.y.ToString();
        eulerZ.text = m_gamecamera.transform.eulerAngles.z.ToString();

        nearHeight.text = m_gamecamera.near.ToString();
        farHeight.text = m_gamecamera.far.ToString();
        smoothTime.text = m_gamecamera.MoveSmoothTime.ToString();
    }

    void MinEulerX()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Pitch -= 1;
        if(m_gamecamera.Pitch < -180)
        {
            m_gamecamera.Pitch = -180;
        }
    }

    void OnEulerXChanged(float value)
    {
        if (!m_gamecamera) return;

        m_gamecamera.Pitch = value;
        if (m_gamecamera.Pitch < -180)
        {
            m_gamecamera.Pitch = -180;
        }
        if (m_gamecamera.Pitch > 180)
        {
            m_gamecamera.Pitch = 180;
        }
    }

    void AddEulerX()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Pitch += 1;
        if (m_gamecamera.Pitch > 180)
        {
            m_gamecamera.Pitch = 180;
        }
    }

    void MinEulerY()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Yaw -= 1;
        if (m_gamecamera.Yaw < -180)
        {
            m_gamecamera.Yaw = -180;
        }
    }

    void EulerYChange(float value)
    {
        if (!m_gamecamera) return;
        m_gamecamera.Yaw = value;
        if (m_gamecamera.Yaw < -180)
        {
            m_gamecamera.Yaw = -180;
        }
        if (m_gamecamera.Yaw > 180)
        {
            m_gamecamera.Yaw = 180;
        }
    }

    void AddEuluerY()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Yaw += 1;
        if (m_gamecamera.Yaw > 180)
        {
            m_gamecamera.Yaw = 180;
        }
    }

    void MinEulerZ()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Roll -= 1;
        if (m_gamecamera.Roll < -180)
        {
            m_gamecamera.Roll = -180;
        }
    }

    void OnEulerZChanged(float value)
    {
        if (!m_gamecamera) return;
        m_gamecamera.Roll = value;
        if (m_gamecamera.Roll < -180)
        {
            m_gamecamera.Roll = -180;
        }
        if (m_gamecamera.Roll > 180)
        {
            m_gamecamera.Roll = 180;
        }
    }

    void AddEulerZ()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Roll += 1;
        if (m_gamecamera.Roll > 180)
        {
            m_gamecamera.Roll = 180;
        }
        
    }


    void MinHeightNear()
    {
        if (!m_gamecamera) return;
        m_gamecamera.near -= 0.01f;
        if (m_gamecamera.near < 0)
        {
            m_gamecamera.near = 0;
        }

        //m_player.near_front.transform.localPosition = new Vector3(0, 0, m_gamecamera.near);
        //m_player.near_back.transform.localPosition = new Vector3(0, 0, m_gamecamera.near);
        //m_player.near_left.transform.localPosition = new Vector3(0, 0, m_gamecamera.near);
        //m_player.near_right.transform.localPosition = new Vector3(0, 0, m_gamecamera.near);
    }

    void OnEHeightNearChanged(float value)
    {
        if (!m_gamecamera) return;
        m_gamecamera.near = value;
        if (m_gamecamera.near < 0)
        {
            m_gamecamera.near = 0;
        }
        if (m_gamecamera.near > 10)
        {
            m_gamecamera.near = 10;
        }
    }

    void AddHeightNear()
    {
        if (!m_gamecamera) return;
        m_gamecamera.near += 0.01f;
        if (m_gamecamera.near > 10)
        {
            m_gamecamera.near = 10;
        }
        //m_player.near_front.transform.localPosition = new Vector3(0,0, m_gamecamera.near);
        //m_player.near_back.transform.localPosition = new Vector3(0, 0, m_gamecamera.near);
        //m_player.near_left.transform.localPosition = new Vector3(0, 0, m_gamecamera.near);
        //m_player.near_right.transform.localPosition = new Vector3(0, 0, m_gamecamera.near);
    }

    void MinHeightFar()
    {
        if (!m_gamecamera) return;
        m_gamecamera.far -= 0.02f;
        if (m_gamecamera.far < 0)
        {
            m_gamecamera.far = 0;
        }
        //m_player.far_front.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
        //m_player.far_back.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
        //m_player.far_left.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
        //m_player.far_right.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
       
    }

    void OnEHeightFarChanged(float value)
    {
        if (!m_gamecamera) return;
        m_gamecamera.far = value;
        if (m_gamecamera.far < 0)
        {
            m_gamecamera.far = 0;
        }
        if (m_gamecamera.far > 10)
        {
            m_gamecamera.far = 10;
        }
    }

    void AddHeightFar()
    {
        if (!m_gamecamera) return;
        m_gamecamera.far += 0.02f;
        if (m_gamecamera.far > 10)
        {
            m_gamecamera.far = 10;
        }
        m_player.far_front.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
        m_player.far_back.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
        m_player.far_left.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
        m_player.far_right.transform.localPosition = new Vector3(0, 0, m_gamecamera.far);
    }

    void MinSmoothTime()
    {
        if (!m_gamecamera) return;
        m_gamecamera.ChangeSmoothTime -= 0.02f;
        if (m_gamecamera.ChangeSmoothTime < 0)
        {
            m_gamecamera.ChangeSmoothTime = 0;
        }
    }

    void onSmoothTimeChanged(float value)
    {
        if (!m_gamecamera) return;
        m_gamecamera.MoveSmoothTime = value;
        if (m_gamecamera.MoveSmoothTime < 0)
        {
            m_gamecamera.MoveSmoothTime = 0;
        }
        if (m_gamecamera.MoveSmoothTime > 10)
        {
            m_gamecamera.MoveSmoothTime = 10;
        }
    }

    void AddSmoothTime()
    {
        if (!m_gamecamera) return;
        m_gamecamera.ChangeSmoothTime += 0.02f;
        if (m_gamecamera.ChangeSmoothTime > 1)
        {
            m_gamecamera.ChangeSmoothTime = 1;
        }
    }


    bool expand = false;
    protected override void OnShow()
    {
        base.OnShow();
        this.SetExpand(true);
    }

    protected override void OnHide()
    {
        base.OnHide();
    }

    void CameraNear()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Near();
    }

    void CameraFar()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Far();
    }

    void ExpandOrHide()
    {
        SetExpand(!expand);
    }

    void SetExpand(bool value)
    {
        expand = value;
        if(expand)
        {
            content.anchoredPosition = new Vector2(-268, 520);
        }
        else
        {
            content.anchoredPosition = new Vector2(268, 520);
        }
    }

    public void InitWithGameCamera(GameCamera gamecamera, Player player)
    {
        m_gamecamera = gamecamera;
        m_player = player;
    }
}
