class CalendarEventModel {
  final int id;
  final String title;
  final String eventType;
  final DateTime dueDate;
  final String courseName;
  final String priority;
  final String? venue;

  CalendarEventModel({
    required this.id,
    required this.title,
    required this.eventType,
    required this.dueDate,
    required this.courseName,
    required this.priority,
    this.venue,
  });

  factory CalendarEventModel.fromJson(Map<String, dynamic> json) {
    return CalendarEventModel(
      id: json['id'] as int,
      title: json['title'] as String,
      eventType: json['eventType'] as String,
      dueDate: DateTime.parse(json['dueDate'] as String),
      courseName: json['courseName'] as String,
      priority: json['priority'] as String,
      venue: json['venue'] as String?,
    );
  }

  Map<String, dynamic> toJson() {
    return {
      'id': id,
      'title': title,
      'eventType': eventType,
      'dueDate': dueDate.toIso8601String(),
      'courseName': courseName,
      'priority': priority,
      'venue': venue,
    };
  }
}
