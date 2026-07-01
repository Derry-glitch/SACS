import 'package:flutter/material.dart';
import '../core/theme/app_theme.dart';

class LoadingSpinner extends StatefulWidget {
  final double size;
  final Color? color;
  
  const LoadingSpinner({
    super.key,
    this.size = 40.0,
    this.color,
  });

  @override
  State<LoadingSpinner> createState() => _LoadingSpinnerState();
}

class _LoadingSpinnerState extends State<LoadingSpinner> with SingleTickerProviderStateMixin {
  late AnimationController _controller;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(seconds: 1),
    )..repeat();
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final activeColor = widget.color ?? AppTheme.primaryLight;

    return RotationTransition(
      turns: _controller,
      child: SizedBox(
        width: widget.size,
        height: widget.size,
        child: CustomPaint(
          painter: _SpinnerPainter(color: activeColor),
        ),
      ),
    );
  }
}

class _SpinnerPainter extends CustomPainter {
  final Color color;

  _SpinnerPainter({required this.color});

  @override
  void paint(Canvas canvas, Size size) {
    final paint = Paint()
      ..color = color
      ..strokeWidth = 3.5
      ..style = PaintingStyle.stroke
      ..strokeCap = StrokeCap.round;

    final rect = Rect.fromLTWH(0, 0, size.width, size.height);
    
    // Draw an arc spanning 280 degrees for a premium spinner feel
    canvas.drawArc(rect, 0, 4.88, false, paint);
  }

  @override
  bool shouldRepaint(covariant CustomPainter oldDelegate) => false;
}

class LoadingSkeletonCard extends StatefulWidget {
  final double height;
  final double width;
  final double borderRadius;

  const LoadingSkeletonCard({
    super.key,
    this.height = 80.0,
    this.width = double.infinity,
    this.borderRadius = 16.0,
  });

  @override
  State<LoadingSkeletonCard> createState() => _LoadingSkeletonCardState();
}

class _LoadingSkeletonCardState extends State<LoadingSkeletonCard> with SingleTickerProviderStateMixin {
  late AnimationController _controller;
  late Animation<double> _gradientPosition;

  @override
  void initState() {
    super.initState();
    _controller = AnimationController(
      vsync: this,
      duration: const Duration(milliseconds: 1500),
    )..repeat();

    _gradientPosition = Tween<double>(begin: -2.0, end: 2.0).animate(
      CurvedAnimation(parent: _controller, curve: Curves.easeInOutSine),
    );
  }

  @override
  void dispose() {
    _controller.dispose();
    super.dispose();
  }

  @override
  Widget build(BuildContext context) {
    final isDark = Theme.of(context).brightness == Brightness.dark;

    return AnimatedBuilder(
      animation: _controller,
      builder: (context, child) {
        return Container(
          width: widget.width,
          height: widget.height,
          decoration: BoxDecoration(
            borderRadius: BorderRadius.circular(widget.borderRadius),
            gradient: LinearGradient(
              begin: Alignment(
                _gradientPosition.value,
                -0.3,
              ),
              end: Alignment(
                _gradientPosition.value + 1.0,
                0.3,
              ),
              colors: isDark
                  ? [
                      const Color(0xFF1E293B),
                      const Color(0xFF334155),
                      const Color(0xFF1E293B),
                    ]
                  : [
                      const Color(0xFFE2E8F0),
                      const Color(0xFFF1F5F9),
                      const Color(0xFFE2E8F0),
                    ],
            ),
          ),
        );
      },
    );
  }
}

class LoadingSkeletonList extends StatelessWidget {
  final int itemCount;
  final double cardHeight;
  
  const LoadingSkeletonList({
    super.key,
    this.itemCount = 3,
    this.cardHeight = 80.0,
  });

  @override
  Widget build(BuildContext context) {
    return ListView.separated(
      shrinkWrap: true,
      physics: const NeverScrollableScrollPhysics(),
      itemCount: itemCount,
      separatorBuilder: (context, index) => const SizedBox(height: 12),
      itemBuilder: (context, index) => LoadingSkeletonCard(height: cardHeight),
    );
  }
}
