#region Used namespaces

using System.Drawing;

using KGySoft.Drawing.Properties;

#endregion

namespace KGySoft.Drawing
{
    /// <summary>
    /// Provides general images applicable for button icons and for other purposes.
    /// Most of the images have 16x16 size. Members with <c>Large</c> ending have 256x256 size.
    /// Members with <c>MultiSize</c> ending are multi-resolution images.
    /// </summary>
    public static class Images
    {
        #region Fields

        private static readonly Size size16 = new Size(16, 16);
        private static readonly Size size256 = new Size(256, 256);

        #endregion

        #region Properties

        /// <summary>
        /// <img src="../Resources/Add.png" alt="Add"/>
        /// White plus in a green circle (16x16)
        /// </summary>
        public static Bitmap Add
        {
            get { return Resources.Add; }
        }

        /// <summary>
        /// <img src="../Resources/Delete.png" alt="Delete"/>
        /// White X in a red circle (16x16)
        /// </summary>
        public static Bitmap Delete
        {
            get { return Resources.Delete; }
        }

        /// <summary>
        /// <img src="../Resources/Refuse.png" alt="Refuse"/>
        /// White minus in a red circle (16x16)
        /// </summary>
        public static Bitmap Refuse
        {
            get { return Resources.Refuse; }
        }

        /// <summary>
        /// <img src="../Resources/Edit.png" alt="Edit"/>
        /// Pencil writing on a sheet of paper (16x16)
        /// </summary>
        public static Bitmap Edit
        {
            get { return Resources.Edit; }
        }

        /// <summary>
        /// <img src="../Resources/Merge.png" alt="Merge"/>
        /// Rows united into one block (16x16)
        /// </summary>
        public static Bitmap Merge
        {
            get { return Resources.Merge; }
        }

        /// <summary>
        /// <img src="../Resources/Accept.png" alt="Accept"/>
        /// White check in a green circle (16x16)
        /// </summary>
        public static Bitmap Accept
        {
            get { return Resources.Accept; }
        }

        /// <summary>
        /// <img src="../Resources/Undo.png" alt="Undo"/>
        /// Blue arrow turning counterclockwise (16x16)
        /// </summary>
        public static Bitmap Undo
        {
            get { return Resources.Undo; }
        }

        /// <summary>
        /// <img src="../Resources/Save.png" alt="Save"/>
        /// Blue floppy (16x16)
        /// </summary>
        public static Bitmap Save
        {
            get { return Resources.Save; }
        }

        /// <summary>
        /// <img src="../Resources/SaveAll.png" alt="Save All"/>
        /// Two blue floppies (16x16)
        /// </summary>
        public static Bitmap SaveAll
        {
            get { return Resources.SaveAll; }
        }

        /// <summary>
        /// <img src="../Resources/Browse.png" alt="Browse"/>
        /// Opened yellow folder (16x16)
        /// </summary>
        public static Bitmap Browse
        {
            get { return Resources.Browse; }
        }

        /// <summary>
        /// <img src="../Resources/All.gif" alt="All"/>
        /// Capital Greek Sigma (Sum) symbol (16x16)
        /// </summary>
        public static Bitmap All
        {
            get { return Resources.All; }
        }

        /// <summary>
        /// <img src="../Resources/ArrowDown.png" alt="Arrow Down"/>
        /// Blue arrow pointing down (16x16)
        /// </summary>
        public static Bitmap ArrowDown
        {
            get { return Resources.ArrowDown; }
        }

        /// <summary>
        /// <img src="../Resources/ArrowUp.png" alt="Arrow Up"/>
        /// Blue arrow pointing up (16x16)
        /// </summary>
        public static Bitmap ArrowUp
        {
            get { return Resources.ArrowUp; }
        }

        /// <summary>
        /// <img src="../Resources/Book.png" alt="Book"/>
        /// Blue book (16x16)
        /// </summary>
        public static Bitmap Book
        {
            get { return Resources.Book; }
        }

        /// <summary>
        /// <img src="../Resources/Box.png" alt="Box"/>
        /// Brown drawer (16x16)
        /// </summary>
        public static Bitmap Box
        {
            get { return Resources.Box; }
        }

        /// <summary>
        /// <img src="../Resources/Clear.png" alt="Clear"/>
        /// Black X (16x16)
        /// </summary>
        public static Bitmap Clear
        {
            get { return Resources.Clear; }
        }

        /// <summary>
        /// <img src="../Resources/Exit.png" alt="Exit"/>
        /// Opened door with green arrow pointing outside (16x16)
        /// </summary>
        public static Bitmap Exit
        {
            get { return Resources.Exit; }
        }

        /// <summary>
        /// <img src="../Resources/Find.png" alt="Find"/>
        /// Blue goggles (16x16)
        /// </summary>
        public static Bitmap Find
        {
            get { return Resources.Find; }
        }

        /// <summary>
        /// <img src="../Resources/Group.png" alt="Group"/>
        /// Two books and a box (16x16)
        /// </summary>
        public static Bitmap Group
        {
            get { return Resources.Group; }
        }

        /// <summary>
        /// <img src="../Resources/Hourglass.png" alt="Hourglass"/>
        /// Hourglass (16x16)
        /// </summary>
        public static Bitmap HourGlass
        {
            get { return Resources.Hourglass; }
        }

        /// <summary>
        /// <img src="../Resources/House.png" alt="House"/>
        /// House (16x16)
        /// </summary>
        public static Bitmap Home
        {
            get { return Resources.House; }
        }

        /// <summary>
        /// <img src="../Help/Images/Information16.png" alt="Information"/>
        /// White "i" in a blue circle (16x16)
        /// </summary>
        public static Bitmap Information
        {
            get { return Icons.InformationIcon.ExtractNearestBitmap(32, size16, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Information16.png" alt="Information (small version for the summary)"/>
        /// White "i" in a blue circle (256x256)
        /// </summary>
        /// <remarks>
        /// The full size icon looks like as displayed here:<br/>
        /// <img src="../Help/Images/Information256.png" alt="Information"/>
        /// </remarks>
        public static Bitmap InformationLarge
        {
            get { return Icons.InformationIcon.ExtractNearestBitmap(32, size256, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Information16.png" alt="Information (small version for the summary)"/>
        /// White "i" a blue circle (Sizes: 256x256, 48x48, 32x32, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bitmap contains the following images:<br/>
        /// <img src="../Help/Images/Information256.png" alt="Information 256x256"/>
        /// <img src="../Help/Images/Information48.png" alt="Information 48x48"/>
        /// <img src="../Help/Images/Information32.png" alt="Information 32x32"/>
        /// <img src="../Help/Images/Information16.png" alt="Information 16x16"/>
        /// </para>
        /// <para>To obtain the images of a multi-size image, use the <see cref="BitmapTools.ExtractBitmaps"/> method.</para>
        /// <para>To get a custom sized image from a multi-size image, use the <see cref="BitmapTools.Resize"/> method.
        /// It will always take the best fitting image for the result.</para>
        /// </remarks>
        public static Bitmap InformationMultiSize
        {
            get { return Icons.InformationIcon.ToBitmap(); }
        }

        /// <summary>
        /// <img src="../Help/Images/Error16.png" alt="Error"/>
        /// White "X" in a red circle (16x16)
        /// </summary>
        public static Bitmap Error
        {
            get { return Icons.ErrorIcon.ExtractNearestBitmap(32, size16, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Error16.png" alt="Error (small version for the summary)"/>
        /// White "X" in a red circle (256x256)
        /// </summary>
        /// <remarks>
        /// The full size icon looks like as displayed here:<br/>
        /// <img src="../Help/Images/Error256.png" alt="Error"/>
        /// </remarks>
        public static Bitmap ErrorLarge
        {
            get { return Icons.ErrorIcon.ExtractNearestBitmap(32, size256, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Error16.png" alt="Error (small version for the summary)"/>
        /// White "X" in a red circle (256x256, 48x48, 32x32, 24x24, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bitmap contains the following images:<br/>
        /// <img src="../Help/Images/Error256.png" alt="Error 256x256"/>
        /// <img src="../Help/Images/Error48.png" alt="Error 48x48"/>
        /// <img src="../Help/Images/Error32.png" alt="Error 32x32"/>
        /// <img src="../Help/Images/Error24.png" alt="Error 24x24"/>
        /// <img src="../Help/Images/Error16.png" alt="Error 16x16"/>
        /// </para>
        /// <para>To obtain the images of a multi-size image, use the <see cref="BitmapTools.ExtractBitmaps"/> method.</para>
        /// <para>To get a custom sized image from a multi-size image, use the <see cref="BitmapTools.Resize"/> method.
        /// It will always take the best fitting image for the result.</para>
        /// </remarks>
        public static Bitmap ErrorMultiSize
        {
            get { return Icons.ErrorIcon.ToBitmap(); }
        }

        /// <summary>
        /// <img src="../Help/Images/Question16.png" alt="Question"/>
        /// White "?" in a blue circle (16x16)
        /// </summary>
        public static Bitmap Question
        {
            get { return Icons.QuestionIcon.ExtractNearestBitmap(32, size16, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Question16.png" alt="Question (small version for the summary)"/>
        /// White "?" in a blue circle (256x256)
        /// </summary>
        /// <remarks>
        /// The full size icon looks like as displayed here:<br/>
        /// <img src="../Help/Images/Question256.png" alt="Question"/>
        /// </remarks>
        public static Bitmap QuestionLarge
        {
            get { return Icons.QuestionIcon.ExtractNearestBitmap(32, size256, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Question16.png" alt="Question (small version for the summary)"/>
        /// White "?" in a blue circle (256x256, 64x64, 48x48, 32x32, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bitmap contains the following images:<br/>
        /// <img src="../Help/Images/Question256.png" alt="Question 256x256"/>
        /// <img src="../Help/Images/Question64.png" alt="Question 64x64"/>
        /// <img src="../Help/Images/Question48.png" alt="Question 48x48"/>
        /// <img src="../Help/Images/Question32.png" alt="Question 32x32"/>
        /// <img src="../Help/Images/Question16.png" alt="Question 16x16"/>
        /// </para>
        /// <para>To obtain the images of a multi-size image, use the <see cref="BitmapTools.ExtractBitmaps"/> method.</para>
        /// <para>To get a custom sized image from a multi-size image, use the <see cref="BitmapTools.Resize"/> method.
        /// It will always take the best fitting image for the result.</para>
        /// </remarks>
        public static Bitmap QuestionMultiSize
        {
            get { return Icons.QuestionIcon.ToBitmap(); }
        }

        /// <summary>
        /// <img src="../Help/Images/Warning16.png" alt="Warning"/>
        /// Black "!" in a yellow triangle (16x16)
        /// </summary>
        public static Bitmap Warning
        {
            get { return Icons.WarningIcon.ExtractNearestBitmap(32, size16, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Warning16.png" alt="Warning (small version for the summary)"/>
        /// Black "!" in a yellow triangle (256x256)
        /// </summary>
        /// <remarks>
        /// The full size icon looks like as displayed here:<br/>
        /// <img src="../Help/Images/Warning256.png" alt="Warning"/>
        /// </remarks>
        public static Bitmap WarningLarge
        {
            get { return Icons.WarningIcon.ExtractNearestBitmap(32, size256, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Warning16.png" alt="Warning (small version for the summary)"/>
        /// Black "!" in a yellow triangle (256x256, 48x48, 32x32, 16x16)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bitmap contains the following images:<br/>
        /// <img src="../Help/Images/Warning256.png" alt="Warning 256x256"/>
        /// <img src="../Help/Images/Warning48.png" alt="Warning 48x48"/>
        /// <img src="../Help/Images/Warning32.png" alt="Warning 32x32"/>
        /// <img src="../Help/Images/Warning16.png" alt="Warning 16x16"/>
        /// </para>
        /// <para>To obtain the images of a multi-size image, use the <see cref="BitmapTools.ExtractBitmaps"/> method.</para>
        /// <para>To get a custom sized image from a multi-size image, use the <see cref="BitmapTools.Resize"/> method.
        /// It will always take the best fitting image for the result.</para>
        /// </remarks>
        public static Bitmap WarningMultiSize
        {
            get { return Icons.WarningIcon.ToBitmap(); }
        }

        /// <summary>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Security Shield"/>
        /// Blue-yellow security shield (16x16)
        /// </summary>
        public static Bitmap SecurityShield
        {
            get { return Icons.SecurityShieldIcon.ExtractNearestBitmap(32, size16, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Security Shield (small version for the summary)"/>
        /// Blue-yellow security shield (256x256)
        /// </summary>
        /// <remarks>
        /// The full size icon looks like as displayed here:<br/>
        /// <img src="../Help/Images/SecurityShield256.png" alt="Security Shield"/>
        /// </remarks>
        public static Bitmap SecurityShieldLarge
        {
            get { return Icons.SecurityShieldIcon.ExtractNearestBitmap(32, size256, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Security Shield (small version for the summary)"/>
        /// Blue-yellow security shield (256x256, 128x128, 48x48, 32x32, 24x24, 16x16, 8x8)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bitmap contains the following images:<br/>
        /// <img src="../Help/Images/SecurityShield256.png" alt="Security Shield 256x256"/>
        /// <img src="../Help/Images/SecurityShield128.png" alt="Security Shield 128x128"/>
        /// <img src="../Help/Images/SecurityShield48.png" alt="Security Shield 48x48"/>
        /// <img src="../Help/Images/SecurityShield32.png" alt="Security Shield 32x32"/>
        /// <img src="../Help/Images/SecurityShield24.png" alt="Security Shield 24x24"/>
        /// <img src="../Help/Images/SecurityShield16.png" alt="Security Shield 16x16"/>
        /// <img src="../Help/Images/SecurityShield8.png" alt="Security Shield 8x8"/>
        /// </para>
        /// <para>To obtain the images of a multi-size image, use the <see cref="BitmapTools.ExtractBitmaps"/> method.</para>
        /// <para>To get a custom sized image from a multi-size image, use the <see cref="BitmapTools.Resize"/> method.
        /// It will always take the best fitting image for the result.</para>
        /// </remarks>
        public static Bitmap SecurityShieldMultiSize
        {
            get { return Icons.SecurityShieldIcon.ToBitmap(); }
        }

        /// <summary>
        /// <img src="../Help/Images/Shield16.png" alt="Shield"/>
        /// Red-green-blue-yellow Windows shield (16x16)
        /// </summary>
        public static Bitmap Shield
        {
            get { return Icons.ShieldIcon.ExtractNearestBitmap(32, size16, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Shield16.png" alt="Shield (small version for the summary)"/>
        /// Red-green-blue-yellow Windows shield (256x256)
        /// </summary>
        /// <remarks>
        /// The full size icon looks like as displayed here:<br/>
        /// <img src="../Help/Images/Shield256.png" alt="Shield"/>
        /// </remarks>
        public static Bitmap ShieldLarge
        {
            get { return Icons.ShieldIcon.ExtractNearestBitmap(32, size256, false); }
        }

        /// <summary>
        /// <img src="../Help/Images/Shield16.png" alt="Shield (small version for the summary)"/>
        /// Red-green-blue-yellow Windows shield (256x256, 128x128, 48x48, 32x32, 24x24, 16x16, 8x8)
        /// </summary>
        /// <remarks>
        /// <para>
        /// The bitmap contains the following images:<br/>
        /// <img src="../Help/Images/Shield256.png" alt="Shield 256x256"/>
        /// <img src="../Help/Images/Shield128.png" alt="Shield 128x128"/>
        /// <img src="../Help/Images/Shield48.png" alt="Shield 48x48"/>
        /// <img src="../Help/Images/Shield32.png" alt="Shield 32x32"/>
        /// <img src="../Help/Images/Shield24.png" alt="Shield 24x24"/>
        /// <img src="../Help/Images/Shield16.png" alt="Shield 16x16"/>
        /// <img src="../Help/Images/Shield8.png" alt="Shield 8x8"/>
        /// </para>
        /// <para>To obtain the images of a multi-size image, use the <see cref="BitmapTools.ExtractBitmaps"/> method.</para>
        /// <para>To get a custom sized image from a multi-size image, use the <see cref="BitmapTools.Resize"/> method.
        /// It will always take the best fitting image for the result.</para>
        /// </remarks>
        public static Bitmap ShieldMultiSize
        {
            get { return Icons.ShieldIcon.ToBitmap(); }
        }

        /// <summary>
        /// <img src="../Resources/Mail.png" alt="Mail"/>
        /// Mail (16x16)
        /// </summary>
        public static Bitmap Mail
        {
            get { return Resources.Mail; }
        }

        /// <summary>
        /// <img src="../Resources/NavBack.png" alt="Navigating Back"/>
        /// White arrow pointing left in a green circle (16x16)
        /// </summary>
        public static Bitmap NavBack
        {
            get { return Resources.NavBack; }
        }

        /// <summary>
        /// <img src="../Resources/NavForward.png" alt="Navigating Forward"/>
        /// White arrow pointing right in a green circle (16x16)
        /// </summary>
        public static Bitmap NavForward
        {
            get { return Resources.NavForward; }
        }

        /// <summary>
        /// <img src="../Resources/New.png" alt="New"/>
        /// Empty white sheet with a glint (16x16)
        /// </summary>
        public static Bitmap New
        {
            get { return Resources.New; }
        }

        /// <summary>
        /// <img src="../Resources/None.png" alt="None"/>
        /// Red slashed circle (16x16)
        /// </summary>
        public static Bitmap None
        {
            get { return Resources.None; }
        }

        /// <summary>
        /// <img src="../Resources/Options.png" alt="Options"/>
        /// White sheet with radio buttons (16x16)
        /// </summary>
        public static Bitmap Options
        {
            get { return Resources.Options; }
        }

        /// <summary>
        /// <img src="../Resources/Redo.png" alt="Redo"/>
        /// Blue arrow turning clockwise (16x16)
        /// </summary>
        public static Bitmap Redo
        {
            get { return Resources.Redo; }
        }

        /// <summary>
        /// <img src="../Resources/Refresh.png" alt="Refresh"/>
        /// Two green arrows forming a circle turning clockwise (16x16)
        /// </summary>
        public static Bitmap Refresh
        {
            get { return Resources.Refresh; }
        }

        /// <summary>
        /// <img src="../Resources/Play.png" alt="Play"/>
        /// Green triangle pointing right (16x16)
        /// </summary>
        public static Bitmap Play
        {
            get { return Resources.Play; }
        }

        /// <summary>
        /// <img src="../Resources/Stop.png" alt="Stop"/>
        /// Blue square (16x16)
        /// </summary>
        public static Bitmap Stop
        {
            get { return Resources.Stop; }
        }

        /// <summary>
        /// <img src="../Resources/Gear.png" alt="Gear"/>
        /// Blue gear (16x16)
        /// </summary>
        public static Bitmap Gear
        {
            get { return Resources.Gear; }
        }

        /// <summary>
        /// <img src="../Resources/Copy.png" alt="Copy"/>
        /// Two sheets with lines (16x16)
        /// </summary>
        public static Bitmap Copy
        {
            get { return Resources.Copy; }
        }

        /// <summary>
        /// <img src="../Resources/Cut.png" alt="Cut"/>
        /// Scissors (16x16)
        /// </summary>
        public static Bitmap Cut
        {
            get { return Resources.Cut; }
        }

        /// <summary>
        /// <img src="../Resources/Paste.png" alt="Paste"/>
        /// A sheet of paper on a board (16x16)
        /// </summary>
        public static Bitmap Paste
        {
            get { return Resources.Paste; }
        }

        /// <summary>
        /// <img src="../Resources/Palette.png" alt="Palette"/>
        /// A palette with color inks and a brush (16x16)
        /// </summary>
        public static Bitmap Palette
        {
            get { return Resources.Palette; }
        }

        /// <summary>
        /// <img src="../Resources/MoveFirst.png" alt="Move First"/>
        /// Blue triangle pointing left to a blue rectangle (16x16)
        /// </summary>
        public static Bitmap MoveFirst
        {
            get { return Resources.MoveFirst; }
        }

        /// <summary>
        /// <img src="../Resources/MoveLast.png" alt="Move Last"/>
        /// Blue triangle pointing right to a blue rectangle (16x16)
        /// </summary>
        public static Bitmap MoveLast
        {
            get { return Resources.MoveLast; }
        }

        /// <summary>
        /// <img src="../Resources/MovePrevious.png" alt="Move Previous"/>
        /// Blue triangle pointing left (16x16)
        /// </summary>
        public static Bitmap MovePrevious
        {
            get { return Resources.MovePrevious; }
        }

        /// <summary>
        /// <img src="../Resources/MoveNext.png" alt="Move Next"/>
        /// Blue triangle pointing right (16x16)
        /// </summary>
        public static Bitmap MoveNext
        {
            get { return Resources.MoveNext; }
        }

        /// <summary>
        /// <img src="../Resources/History.png" alt="History"/>
        /// Sheet with a small clock (16x16)
        /// </summary>
        public static Bitmap History
        {
            get { return Resources.History; }
        }

        /// <summary>
        /// <img src="../Resources/Clock.png" alt="Clock"/>
        /// Clock (16x16)
        /// </summary>
        public static Bitmap Clock
        {
            get { return Resources.Clock; }
        }

        /// <summary>
        /// <img src="../Resources/Magnifier.png" alt="Magnifier"/>
        /// Magnifier (16x16)
        /// </summary>
        public static Bitmap Magnifier
        {
            get { return Resources.Magnifier; }
        }

        /// <summary>
        /// <img src="../Resources/Pause.png" alt="Pause"/>
        /// Two blue portrait rectangles (16x16)
        /// </summary>
        public static Bitmap Pause
        {
            get { return Resources.Pause; }
        }

        #endregion
    }
}
