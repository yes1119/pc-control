   M S B A M L   `   `   `  ����H  DMahApps.Metro, Version=0.9.0.0, Culture=neutral, PublicKeyToken=nullV;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.MetroMahApps.Metro.Controls  5      O Kmscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e0891&clr-namespace:System;assembly=mscorlibSystem 5	      	 Admin   Admin.Dialog_Get.LogInfo   5      \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35D 9http://schemas.microsoft.com/winfx/2006/xaml/presentation    5      R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      PControls;clr-namespace:MahApps.Metro.Controls;assembly=MahApps.Metro     5      9System&clr-namespace:System;assembly=mscorlib     5	        � Title$	  Log��5       � 
ResizeMode$ NoResize=�5       � WindowStyle$
 None=�5       � WindowStartupLocation$ CenterScreen=�5      $	��600��5
      $	��800��6$   (   "MahApps.Metro.Controls.MetroWindow   ShowMaxRestoreButton .     TitleForeground �#999988.��5      � .+�5   
   �� .@�5      �� -   5        $��tab_setting��5      $
��Left=�6)   $��85��6D    �� 
IsSelected . .��5      >� .��5      �� -   5        $
��lvw1��5      $
��Left=�6+   $	��Top=�6F   ��6_   �� ��5      �� 5       �� 
TargetType" ��6$   .�5   "   ��  �� Property � ��	 �� Value$	 Center=�6O   5      
 �� View
 5      �� 5      .P�5   "   �� $��IP��61    �� Width$	 140��6=    �� DisplayMemberBinding 6I   �� *PC_IP+6s   �� 5   "   $��
층 이름��61   $	 140��6i    6?   �� *
LOG_NAME+6w   �� 5   "   $��사용 시간��61   $	 170��6@    6L   �� *LOG_DETECTION_DAY+6�   �� 5   "   $��최근 시작 시간��61   $	 170��6C    6O   �� *	LOG_LOG+6{   �� 5   "   $��최근 종료 시간��61   $	 170��6C    6O   �� *LOG_DETECTION_TIME+6�   5      5       5!      5"      �� 5#      -   5        $��tab_monitoring��5#      $
��Left=�6,   $	��130��6G    .  .��5$      >� .��5%      �� -   5        $
��lvw2��5%      $
��Left=�6+   $	��Top=�6F   ��6_   �� ��5'      �� 5(      " ��6$   .�5)   "   ��  � ��$	 Center=�6O   5*      
 5-      �� 5.      .P�5/   "   �� $��IP��61   $	 105��6=    6I   �� *PC_IP+6s   �� 50   "   $��소프트웨어��61   $	 185��6j    6@   �� *
LOG_NAME+6x   �� 51   "   $��사용시간��61   $	 165��6?    6K   �� *LOG_DETECTION_DAY+6�   �� 52   "   $��최근 실행 날짜��61   $	 165��6C    6O   �� *	LOG_LOG+6{   �� 53   "   $��최근 실행시간��61   $	 165��6B    6N   �� *LOG_DETECTION_TIME+6�   54      56      57      58      �� 59      -   5        $��tab_control��59      $
��Left=�6*   $	��118��6E    .  .��5:      >� .��5;      �� -   5        $
��lvw3��5;      $
��Left=�6+   $	��Top=�6F   ��6_   �� ��5=      �� 5>      " ��6$   .�5?   "   ��  � ��$	 Center=�6O   5@      
 5C      �� 5D      .P�5E   "   �� $��타입��61   $	 120��6=    6I   �� *
LOG_TYPE+6u   �� 5F   "   $��IP��61   $	 100��6=    6I   �� *PC_IP+6s   �� 5G   "   $��LOG_NAME��61   $	 140��6m    6C   �� *
LOG_NAME+6{   �� 5H   "   $	��LOG��61   $	 180��6>    6J   �� *	LOG_LOG+6v   �� 5I   "   $��날짜��61   $	 130��6=    6I   �� *LOG_DETECTION_DAY+6   �� 5J   "   $��시간��61   $	 120��6=    6I   �� *LOG_DETECTION_TIME+6�   5K      5M      5N      5O      5P      5Q      5R      