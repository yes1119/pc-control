   M S B A M L   `   `   `  ����c  _System.Windows.Interactivity, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35qPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.InteractivitySystem.Windows.Interactivity  5      @ <Admin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null+clr-namespace:Admin.FolderAdmin.Folder 5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089  �� MergedDictionaries �� Source �� 
TargetType   StandardTabControl �� 	Resources �� Property �� Value �� 
TargetType J� SnapsToDevicePixels � ColumnDefinitions	 � 	Resources  
Foreground.    (System.Windows.Interactivity.Interaction
    Triggers/   )System.Windows.Interactivity.EventTrigger   	EventName.   (System.Windows.Interactivity.TriggerBase   Actions%  Admin.Folder.CloseTabItemAction   TabItem �� RelativeSource �� AncestorType   
TabControl F�Name  CloseButtonStroke �� StrokeStartLineCap �� StrokeEndLineCap �� Stretch �� Triggers ^� 
Conditions �� Binding �� Mode �� Value P� Property P� Value $  CloseButtonBackgroundHighlighted �� 
TargetName   CloseButtonStrokeHighlighted   CloseButtonBackgroundPressed  BorderBrushSelected  BackgroundSelected-  'System.Windows.Input.KeyboardNavigation   TabNavigation � RowDefinitions K� ZIndex  K� IsItemsHost!   TabIndex  
Background�� 5      D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    :x,http://schemas.microsoft.com/winfx/2006/xaml     5      ViPclr-namespace:System.Windows.Interactivity;assembly=System.Windows.Interactivity  5      &Actionsclr-namespace:Admin.Folder  5        5      �� 5   
   $; 5pack://application:,,,/Admin;component/Resources.xaml?�6   %P  &	        7  7  7  7  7  7  7  7  7  7  �� 5      " ��6    5	   
   ���� 5
      '��       " ��6   .�5      ��  �@�FocusVisualStyle 65   Z� �� 5       � ��
 ���� 6/   �� 5       � �� 5      �� 5      " ��6*   .R�5      � #��z��6#    . $��23��6h    5   "   �� 5   &   $
��Auto �67   �� 5   &   $��25 �67   .+�5   "   �� $��Header��63   $��	10,0,10,0q�6J   $��Center=�6]   $��Center=�6z   	 5   &   ���� 5   *   '��       " ��60   .�5   .   ��  � ��8   6K   5   +   5   '   5   #   �� 5   "   $��1��6)   $��15��69   $��15��6E   $��Center=�6P   $��Center=�6m   
 5   &     5   *   $ Click��69   . 5    .      6I   ��  �� $ TabItemL� 5!   I   ��  �� $ 
TabControlL�5#   /   ��5%   &   �� 5&   *   " ��60   .�5'   .   ��  � ��
 ���� 6K   �� 5(   .    �@�Cursor$
 Hand��6G   �� 5)   .    � |� .  6J   �� 5*   .    � �� 5+   2   �� 5,   6   " ��6F   .R�5-   :   � #��z��6?   .+�5.   >   F� $ 
ButtonPath��6C   $��2q�5/   A   +����        �@B @B �    @B �@B     	8�� $��2��52   A   $
 Flat=�53   A   $
 Flat=�54   A   $ Uniform=�55   A   $��Center=�56   A   $��Center=�57   A   58   ;    59   :   ^� 5:   >    5;   B   �� 5<   F    6P   �� *
IsSelected+ �� $ FindAncestor=�$ TabItemL�	 false6�   �� 5=   F    6P   �� *IsMouseOver+ �� $ FindAncestor=�$ TabItemL�	 false6�   0�5?   B   �� 5@   F    � y�$ Hidden=�6c   5B   ?   P� 5C   >    � {� .  . �5D   B   ��  � y�$ Hidden=�6_   5E   ?   P� 5F   >    �@J�IsMouseOver . . �5G   B   �� 8   � ��6_   �� 5H   B   $ 
ButtonPath��6I   8   � ��6s   5I   ?   P� 5J   >    � �� . . �5K   B   �� 8   � ��6_   �� 5L   B   $ 
ButtonPath��6I   8   � ��6s   �� 5M   B   $ 
ButtonPath��6I    � ��$ 2.5,2.5,1.5,1.5q�6s   5N   ?   5P   7   5R   /   5S   +   5U   #   5V       5W      P� 5X   "    �@��
IsSelected .  . �5Y   &   ��  � ��
 ���� 6C   5Z   #   P� 5[   "    �@J�IsMouseOver . . �5\   &   �� 8   � ��6C   5]   #   P� 5^   "    �@��
IsSelected . . �5_   &   �� 8   � ��6C   5`   #   5b      5d      5e      5f      .�5g   
   ��  � �� 5h      �� 5i      " ��6"   .R�5j      � $ Local=�6    5k      �� 5l      $
��Auto �6,   �� 5m      $��* �6,   .+�5o      �� 
������ $��0,0,0,3q�6:   8�� ._�5q      �� $��HeaderPanel��6'   $ 1��6:   $��0,0,4,-1q�6K     . $! 1��5r   4   5s      �� 5t      $��1��6!   8��	 6.   �� 5u      $��PART_SelectedContentHost��68   $��1��6+   $��SelectedContent��5v   +   5w      5x      5z      5{      5|      