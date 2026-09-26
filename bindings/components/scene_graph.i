%include <tesseract/scene_graph/joint.h>
%include <tesseract/scene_graph/link.h>
%template(VisualVector) std::vector<std::shared_ptr<tesseract::scene_graph::Visual>>;

%rename(getVisuals) tesseract::scene_graph::Link::getVisualsForBinding;
%extend tesseract::scene_graph::Link
{
  std::vector<std::shared_ptr<tesseract::scene_graph::Visual>> getVisualsForBinding() const
  {
    return $self->visual;
  }
}

%include <tesseract/scene_graph/graph.h>
%include <tesseract/scene_graph/scene_state.h>
