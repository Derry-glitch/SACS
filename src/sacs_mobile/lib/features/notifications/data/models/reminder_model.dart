class ReminderModel {
  final int id;
  final int eventId;
  final String eventTitle;
  final String reminderType;
  final DateTime scheduledTime;
  final String status;

  ReminderModel({
    required this.id,
    required this.eventId,
    required this.eventTitle,
    required this.reminderType,
    required this.scheduledTime,
    required this.status,
  });

  factory ReminderModel.fromJson(Map<String, dynamic> json) {
    return ReminderModel(
      id: json['id'] as int,
      eventId: json['eventId'] as int,
      eventTitle: json['eventTitle'] as String,
      reminderType: json['reminderType'] as String,
      scheduledTime: DateTime.parse(json['scheduledTime'] as String),
      status: json['status'] as String,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'eventId': eventId,
      'eventTitle': eventTitle,
      'reminderType': reminderType,
      'scheduledTime': scheduledTime.toIso8601String(),
      'status': status,
    };
  }
}
