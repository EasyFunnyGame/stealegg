using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraSettingCanvas : BaseCanvas
{
    public Button btn_show_and_hide;

    public RectTransform content;

    public Text eulerX;

    public Text eulerY;

    public Text eulerZ;

    public Text up;

    public Text down;

    public Text left;

    public Text right;

    public Text smoothTime;

    public Text height;

    public Button btn_euler_x_min;
    public Button btn_euler_x_add;

    public Button btn_euler_y_min;
    public Button btn_euler_y_add;

    public Button btn_euler_z_min;
    public Button btn_euler_z_add;

    public Button btn_smooth_min;
    public Button btn_smooth_add;

    public Button btn_up_min;
    public Button btn_up_add;

    public Button btn_down_min;
    public Button btn_down_add;

    public Button btn_left_min;
    public Button btn_left_add;

    public Button btn_right_min;
    public Button btn_right_add;

    public Button btn_height_min;
    public Button btn_height_add;

    public GameCamera m_gamecamera;

    public Player m_player;

    // Start is called before the first frame update
    void Start()
    {
        btn_show_and_hide.onClick.AddListener(ExpandOrHide);

        btn_euler_x_add.onClick.AddListener(AddEulerX); 
        btn_euler_x_min.onClick.AddListener(MinEulerX);

        btn_euler_y_add.onClick.AddListener(AddEuluerY);
        btn_euler_y_min.onClick.AddListener(MinEulerY); 

        btn_euler_z_add.onClick.AddListener(AddEulerZ); 
        btn_euler_z_min.onClick.AddListener(MinEulerZ);


        btn_smooth_min.onClick.AddListener(MinSmoothTime);
        btn_smooth_add.onClick.AddListener(AddSmoothTime);

        btn_up_min.onClick.AddListener(MinUp);
        btn_up_add.onClick.AddListener(AddUp);

        btn_down_min.onClick.AddListener(MinDown);
        btn_down_add.onClick.AddListener(AddDown);

        btn_left_min.onClick.AddListener(MinLeft);
        btn_left_add.onClick.AddListener(AddLeft);

        btn_right_min.onClick.AddListener(MinRight);
        btn_right_add.onClick.AddListener(AddRight);

        btn_height_min.onClick.AddListener(MinHeight);
        btn_height_add.onClick.AddListener(AddHeight);

    }


    private void MinHeight()
    {
        if (!m_gamecamera) return;
        m_gamecamera.height -= 0.1f;
    }

    private void AddHeight()
    {
        if (!m_gamecamera) return;
        m_gamecamera.height += 0.1f;
    }


    private void MinUp()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingUp -= 10;
    }
    private void AddUp()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingUp += 10;
    }
    private void MinDown()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingDown -= 10;
    }
    private void AddDown()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingDown += 10;
    }
    private void MinLeft()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingLeft -= 10;
    }
    private void AddLeft()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingLeft += 10;
    }
    private void MinRight()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingRight -= 10;
    }

    private void AddRight()
    {
        if (!m_gamecamera) return;
        m_gamecamera.PaddingRight += 10;
    }

    // Update is called once per frame
    void Update()
    {
        if (!expand) return;
        if (!m_gamecamera) return;
        if (!m_player) return;

        eulerX.text = m_gamecamera.transform.eulerAngles.x.ToString("#0");
        eulerY.text = m_gamecamera.transform.eulerAngles.y.ToString("#0");
        eulerZ.text = m_gamecamera.transform.eulerAngles.z.ToString("#0");

        smoothTime.text = m_gamecamera.MoveSmoothTime.ToString("#0.000");

        up.text = m_gamecamera.PaddingUp.ToString("#0");
        down.text = m_gamecamera.PaddingDown.ToString("#0");
        left.text = m_gamecamera.PaddingLeft.ToString("#0");
        right.text = m_gamecamera.PaddingRight.ToString("#0");

        height.text = m_gamecamera.height.ToString("#0.0");
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

    void AddEulerZ()
    {
        if (!m_gamecamera) return;
        m_gamecamera.Roll += 1;
        if (m_gamecamera.Roll > 180)
        {
            m_gamecamera.Roll = 180;
        }
        
    }

    void MinSmoothTime()
    {
        if (!m_gamecamera) return;
        m_gamecamera.MoveSmoothTime -= 0.001f;
        if (m_gamecamera.MoveSmoothTime < 0)
        {
            m_gamecamera.MoveSmoothTime = 0;
        }
    }

    void AddSmoothTime()
    {
        if (!m_gamecamera) return;
        m_gamecamera.MoveSmoothTime += 0.001f;
        if (m_gamecamera.MoveSmoothTime > 1)
        {
            m_gamecamera.MoveSmoothTime = 1;
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
    }

    void CameraFar()
    {
        if (!m_gamecamera) return;
    }

    void ExpandOrHide()
    {
        SetExpand(!expand);
    }

    public void SetExpand(bool value)
    {
        expand = value;
        if(expand)
        {
            content.anchoredPosition = new Vector2(-180, -350);
        }
        else
        {
            content.anchoredPosition = new Vector2(180, -350);
        }
    }

    public void InitWithGameCamera(GameCamera gamecamera, Player player)
    {
        m_gamecamera = gamecamera;
        m_player = player;
    }
}
